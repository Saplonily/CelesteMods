using System.Reflection;
using Mono.Cecil.Cil;
using Celeste.Mod;
using VivHelperEntities = global::VivHelper.Entities;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BetterFreezeFrames;

public partial class BetterFreezeFramesModule : EverestModule
{
    public static BetterFreezeFramesModule Instance { get; private set; }

    public override Type SettingsType => typeof(BetterFreezeFramesSettings);
    public static BetterFreezeFramesSettings Settings => (BetterFreezeFramesSettings)Instance._Settings;

    public static bool FreezeUpdating;

    public readonly static FieldInfo FreezeUpdatingField =
        typeof(BetterFreezeFramesModule).GetField(nameof(FreezeUpdating));

    // TimeActive doesn't update while freezing
    public static float ExtraTimeActive;

    public readonly static FieldInfo ExtraTimeActiveField =
        typeof(BetterFreezeFramesModule).GetField(nameof(ExtraTimeActive));

    public static bool FemtoHelperLoaded;
    public static bool IsaGrabBagLoaded;
    public static bool VivHelperLoaded;
    public static bool FlaglinesAndSuchLoaded;
    public static bool VortexHelperLoaded;

    private static ILHook VortexHelperBarrierILHook;

    public BetterFreezeFramesModule()
    {
        Instance = this;
    }

    public override void Load()
    {
        if (Settings.DebugEnabled)
            LoadDebug();
        if (Settings.Enabled)
        {
            LoadMain();
        }
        FemtoHelperLoaded = Everest.Loader.TryGetDependency(new() { Name = "FemtoHelper", Version = new(1, 11, 5) }, out _);
        IsaGrabBagLoaded = Everest.Loader.TryGetDependency(new() { Name = "IsaGrabBag", Version = new(1, 6, 14) }, out _);
        VivHelperLoaded = Everest.Loader.TryGetDependency(new() { Name = "VivHelper", Version = new(1, 14, 5) }, out _);
        FlaglinesAndSuchLoaded = Everest.Loader.TryGetDependency(new() { Name = "FlaglinesAndSuch", Version = new(1, 6, 19) }, out _);
        VortexHelperLoaded = Everest.Loader.TryGetDependency(new() { Name = "VortexHelper", Version = new(1, 2, 14) }, out _);
    }

    public override void Unload()
    {
        if (Settings.DebugEnabled)
            UnloadDebug();
        if (Settings.Enabled)
        {
            UnloadMain();
        }
    }

    public void LoadMain()
    {
        IL.Monocle.Scene.Begin += Scene_Begin;
        IL.Monocle.Engine.Update += Engine_Update;
        On.Celeste.ScreenWipe.Update += ScreenWipe_Update;
        IL.Celeste.DreamBlock.Update += SkipBaseUpdateILHook<Solid>;
        On.Celeste.TriggerSpikes.Update += TriggerSpikes_Update;
        IL.Celeste.LightningRenderer.Update += ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update += ILHookReplaceOnInterval;
        IL.Celeste.Refill.Update += SkipBaseUpdateILHook<Entity>;
        if (VortexHelperLoaded)
            LoadVortexHelper();
    }

    public void UnloadMain()
    {
        IL.Monocle.Scene.Begin -= Scene_Begin;
        IL.Monocle.Engine.Update -= Engine_Update;
        On.Celeste.ScreenWipe.Update -= ScreenWipe_Update;
        IL.Celeste.DreamBlock.Update -= SkipBaseUpdateILHook<Solid>;
        On.Celeste.TriggerSpikes.Update -= TriggerSpikes_Update;
        IL.Celeste.LightningRenderer.Update -= ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update -= ILHookReplaceOnInterval;
        IL.Celeste.Refill.Update -= SkipBaseUpdateILHook<Entity>;
        if (VortexHelperLoaded)
            UnloadVortexHelper();
    }

    public void LoadVortexHelper()
    {
        VortexHelperBarrierILHook = new ILHook(typeof(VortexHelper.Entities.PufferBarrierRenderer).GetMethod("Update"), ILHookReplaceOnInterval);
    }

    public void UnloadVortexHelper()
    {
        VortexHelperBarrierILHook?.Dispose();
    }

    public void LoadDebug()
    {
        On.Celeste.Level.Update += Level_Update;
    }

    public void UnloadDebug()
    {
        On.Celeste.Level.Update -= Level_Update;
    }

    private static void Scene_Begin(ILContext il)
    {
        ILCursor cur = new(il);
        cur.EmitLdcR4(0f);
        cur.EmitStsfld(ExtraTimeActiveField);
    }

    private static void ILHookReplaceOnInterval(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(ins => ins.MatchCallvirt<Scene>("OnInterval")))
        {
            var ins = cur.Instrs[cur.Index];
            ins.OpCode = OpCodes.Call;
            ins.Operand = typeof(BetterFreezeFramesModule).GetMethod(nameof(OnExtraInterval));
        }
    }

    private static void SkipBaseUpdateILHook<T>(ILContext il)
    {
        ILCursor cur = new(il);
        cur.GotoNext(MoveType.Before, ins => ins.MatchLdarg0(), ins => ins.MatchCall<T>("Update"));
        Instruction ins = cur.Instrs[cur.Index + 2];
        cur.EmitLdsfld(FreezeUpdatingField);
        cur.EmitBrtrue(ins);
    }

    #region extra patch

    private static void TriggerSpikes_Update(On.Celeste.TriggerSpikes.orig_Update orig, TriggerSpikes self)
    {
        if (FreezeUpdating)
        {
            for (int i = 0; i < self.spikes.Length; i++)
            {
                self.spikes[i].TentacleFrame += Engine.DeltaTime * 12f;
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void ScreenWipe_Update(On.Celeste.ScreenWipe.orig_Update orig, ScreenWipe self, Scene scene)
    {
        if (scene is Level)
        {
            if (!FreezeUpdating)
                orig(self, scene);
        }
        else
        {
            orig(self, scene);
        }
    }

    #endregion

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (Input.MenuJournal.Check)
        {
            Celeste.Freeze(0.05f);
            return;
        }
        orig(self);
    }

    private static void Engine_Update(ILContext il)
    {
        ILCursor cur = new(il);
        cur.TryGotoNext(MoveType.After, ins => ins.MatchStsfld("Monocle.Engine", "FreezeTimer"));
        {
            // ExtraTimeActive += Engine.DeltaTime;
            cur.EmitCall(typeof(Engine).GetProperty(nameof(Engine.DeltaTime)).GetGetMethod());
            cur.EmitLdsfld(ExtraTimeActiveField);
            cur.EmitAdd();
            cur.EmitStsfld(ExtraTimeActiveField);

            // FreezeUpdating = true;
            // self.scene.RendererList.Update();
            // FreezeUpdate(self.scene);
            // FreezeUpdating = false;
            cur.EmitLdcI4(1);
            cur.EmitStsfld(FreezeUpdatingField);
            cur.EmitLdarg0();
            cur.EmitLdfld(typeof(Engine).GetField("scene", BindingFlags.NonPublic | BindingFlags.Instance));
            cur.EmitDup();
            cur.EmitCall(typeof(Scene).GetProperty("RendererList").GetGetMethod());
            cur.EmitCall(typeof(RendererList).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance));
            cur.EmitDelegate(FreezeUpdate);
            cur.EmitLdcI4(0);
            cur.EmitStsfld(FreezeUpdatingField);
        }
        cur.Index = 0;
        cur.TryGotoNext(MoveType.After, ins => ins.MatchCallvirt<Scene>("BeforeUpdate"));
        {
            // ExtraTimeActive += Engine.DeltaTime;
            cur.EmitCall(typeof(Engine).GetProperty(nameof(Engine.DeltaTime)).GetGetMethod());
            cur.EmitLdsfld(ExtraTimeActiveField);
            cur.EmitAdd();
            cur.EmitStsfld(ExtraTimeActiveField);
        }
    }

    private static bool IsSafeToUpdate(Entity entity)
    {
        return CheckVanilla(entity) ||
            (FemtoHelperLoaded && CheckFemtoHelper(entity)) ||
            (IsaGrabBagLoaded && CheckIsaGrabBag(entity)) ||
            (VivHelperLoaded && CheckVivHelper(entity)) ||
            (FlaglinesAndSuchLoaded && CheckFlaglinesAndSuch(entity)) ||
            (VortexHelperLoaded && CheckVortexHelper(entity))
            ;

        static bool CheckVanilla(Entity entity)
            => entity is ParticleSystem
                or SpeedRing
                or WallBooster
                or TrailManager.Snapshot
                or SeekerBarrier
                or SeekerBarrierRenderer // with extra patch
                or DustEdges
                or TouchSwitch
                or WaterFall
                or LightBeam
                or LightningStrike
                or LightningRenderer // with extra patch
                or DreamBlock // with extra patch, may be incompatible with helper dreamblocks
                or FloatingDebris
                or Decal
                or TriggerSpikes // with extra patch
                or Water // may be incompatible with helper water
                or ForegroundDebris
                or Clothesline
                or CliffFlags
                or BigWaterfall
                or MoonCreature
                or Wire
                ;

        static bool CheckVivHelper(Entity entity)
            => entity is VivHelperEntities.CustomTorch or VivHelperEntities.CustomTorch2 or VivHelperEntities.AnimatedSpinner;

        static bool CheckFemtoHelper(Entity entity)
            => entity is FemtoHelper.PseudoPolyhedron or CustomMoonCreature; // wtf it's in global namespace

        static bool CheckIsaGrabBag(Entity entity)
            => entity is IsaGrabBag.DreamSpinnerRenderer;

        static bool CheckFlaglinesAndSuch(Entity entity)
            => entity is FlaglinesAndSuch.CustomFlagline;

        static bool CheckVortexHelper(Entity entity)
            => entity is VortexHelper.Entities.PufferBarrierRenderer or VortexHelper.Entities.PufferBarrier;
    }

    private static void FreezeUpdate(Scene scene)
    {
        foreach (var entity in scene)
        {
            // update "safe to update" entities here
            if (IsSafeToUpdate(entity))
            {
                entity.Update();
                continue;
            }
            switch (entity)
            {
            case Refill refill:
            {
                float orig = refill.respawnTimer;
                refill.respawnTimer = float.PositiveInfinity;
                refill.Update();
                refill.respawnTimer = orig;
                break;
            }
            }
        }
        if (scene is Level level)
        {
            level.WindSineTimer += Engine.DeltaTime;
            level.WindSine = (float)(Math.Sin((double)level.WindSineTimer) + 1.0) / 2f;
        }
        return;
    }

    public static bool OnExtraInterval(Scene _, float interval)
        => (int)((ExtraTimeActive - (double)Engine.DeltaTime) / (double)interval) < (int)(ExtraTimeActive / (double)interval);
}