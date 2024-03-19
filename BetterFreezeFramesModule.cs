using System.Reflection;
using Mono.Cecil.Cil;

namespace Celeste.Mod.BetterFreezeFrames;

public class BetterFreezeFramesModule : EverestModule
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

    // TODO: a better way to control this (currently these hooks is actually done in the setters of property of ModuleSettings
    public static bool LoadedStuffs = false;


    public BetterFreezeFramesModule()
    {
        Instance = this;
    }

    public override void Load()
    {
        if (!Settings.Enabled) return;
        if (LoadedStuffs) return;
#if DEBUG
        On.Celeste.Level.Update += Level_Update;
#endif
        IL.Monocle.Scene.Begin += Scene_Begin;
        IL.Monocle.Engine.Update += Engine_Update;
        On.Celeste.ScreenWipe.Update += ScreenWipe_Update;
        On.Celeste.DreamBlock.Update += DreamBlock_Update;
        On.Celeste.TriggerSpikes.Update += TriggerSpikes_Update;
        IL.Celeste.LightningRenderer.Update += ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update += ILHookReplaceOnInterval;
        LoadedStuffs = true;
    }

    public override void Unload()
    {
        if (!LoadedStuffs) return;
#if DEBUG
        On.Celeste.Level.Update -= Level_Update;
#endif
        IL.Monocle.Scene.Begin -= Scene_Begin;
        IL.Monocle.Engine.Update -= Engine_Update;
        On.Celeste.ScreenWipe.Update -= ScreenWipe_Update;
        On.Celeste.DreamBlock.Update -= DreamBlock_Update;
        On.Celeste.TriggerSpikes.Update -= TriggerSpikes_Update;
        IL.Celeste.LightningRenderer.Update -= ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update -= ILHookReplaceOnInterval;
        LoadedStuffs = false;
    }

    private void Scene_Begin(ILContext il)
    {
        ILCursor cur = new(il);
        cur.EmitLdcR4(0f);
        cur.EmitStsfld(ExtraTimeActiveField);
    }

    private void ILHookReplaceOnInterval(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(ins => ins.MatchCallvirt<Scene>("OnInterval")))
        {
            var ins = cur.Instrs[cur.Index];
            ins.OpCode = OpCodes.Call;
            ins.Operand = typeof(BetterFreezeFramesModule).GetMethod(nameof(OnExtraInterval));
        }
    }

    #region extra patch


    private void TriggerSpikes_Update(On.Celeste.TriggerSpikes.orig_Update orig, TriggerSpikes self)
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

    private void DreamBlock_Update(On.Celeste.DreamBlock.orig_Update orig, DreamBlock self)
    {
        if (!FreezeUpdating)
        {
            orig(self);
            return;
        }
        // TODO: this is silly copying
        if (self.playerHasDreamDash)
        {
            self.animTimer += 6f * Engine.DeltaTime;
            self.wobbleEase += Engine.DeltaTime * 2f;
            if (self.wobbleEase > 1f)
            {
                self.wobbleEase = 0f;
                self.wobbleFrom = self.wobbleTo;
                self.wobbleTo = Calc.Random.NextFloat(MathHelper.TwoPi);
            }
            self.SurfaceSoundIndex = 12;
        }
    }

    #endregion

#if DEBUG
    private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (Input.MenuJournal.Check)
        {
            Celeste.Freeze(0.1f);
            return;
        }
        orig(self);
    }
#endif

    private void Engine_Update(ILContext il)
    {
        ILCursor cur = new(il);
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchStsfld("Monocle.Engine", "FreezeTimer")))
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
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchCallvirt<Scene>("BeforeUpdate")))
        {
            // ExtraTimeActive += Engine.DeltaTime;
            cur.EmitCall(typeof(Engine).GetProperty(nameof(Engine.DeltaTime)).GetGetMethod());
            cur.EmitLdsfld(ExtraTimeActiveField);
            cur.EmitAdd();
            cur.EmitStsfld(ExtraTimeActiveField);
        }
    }

    private static void FreezeUpdate(Scene scene)
    {
        foreach (var entity in scene)
        {
            // update "safe to update" entities here
            if (
            entity is ParticleSystem
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
                or Water // may be incompatible with helper dreamblocks
            )
            {
                entity.Update();
                continue;
            }
            // update "maybe safe" entities here
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
        return;
    }

    public static bool OnExtraInterval(Scene _, float interval)
        => (int)((ExtraTimeActive - (double)Engine.DeltaTime) / (double)interval) < (int)(ExtraTimeActive / (double)interval);
}