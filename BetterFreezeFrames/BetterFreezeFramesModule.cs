using System.Reflection;
using MonoMod;
using MonoMod.Cil;

namespace Celeste.Mod.BetterFreezeFrames;

public partial class BetterFreezeFramesModule : EverestModule
{
    public static BetterFreezeFramesModule Instance { get; private set; }

    public override Type SettingsType => typeof(BetterFreezeFramesSettings);
    public static BetterFreezeFramesSettings Settings => (BetterFreezeFramesSettings)Instance._Settings;

    public static bool FreezeUpdating;

    // TimeActive doesn't update while freezing
    public static float ExtraTimeActive;

    public readonly static FieldInfo FreezeUpdatingField =
        typeof(BetterFreezeFramesModule).GetField(nameof(FreezeUpdating));

    public readonly static FieldInfo ExtraTimeActiveField =
        typeof(BetterFreezeFramesModule).GetField(nameof(ExtraTimeActive));

    public override void Load()
    {
        Instance = this;
        HelpersModule.CheckLoadedStates();
        if (Settings.DebugEnabled)
            LoadDebug();
        if (Settings.Enabled)
            LoadMain();
    }

    public override void Unload()
    {
        if (Settings.DebugEnabled)
            UnloadDebug();
        if (Settings.Enabled)
            UnloadMain();
    }

    public static void LoadMain()
    {
        IL.Monocle.Scene.Begin += Scene_Begin;
        IL.Monocle.Engine.Update += Engine_Update;
        VanillaModule.Load();
        HelpersModule.Load();
    }

    public static void UnloadMain()
    {
        IL.Monocle.Scene.Begin -= Scene_Begin;
        IL.Monocle.Engine.Update -= Engine_Update;
        VanillaModule.Unload();
        HelpersModule.Unload();
    }

    public static void LoadDebug()
        => On.Celeste.Level.Update += Level_Update;

    public static void UnloadDebug()
        => On.Celeste.Level.Update -= Level_Update;

    private static void Scene_Begin(ILContext il)
    {
        ILCursor cur = new(il);
        cur.EmitLdcR4(0f);
        cur.EmitStsfld(ExtraTimeActiveField);
    }

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
        cur.GotoNext(MoveType.After, ins => ins.MatchStsfld("Monocle.Engine", "FreezeTimer"));
        {
            /*
             *  ExtraTimeActive += Engine.DeltaTime; 
             */
            cur.EmitCall(typeof(Engine).GetProperty(nameof(Engine.DeltaTime)).GetGetMethod());
            cur.EmitLdsfld(ExtraTimeActiveField);
            cur.EmitAdd();
            cur.EmitStsfld(ExtraTimeActiveField);

            /* 
             *  FreezeUpdating = true;
             *  self.scene.RendererList.Update();
             *  FreezeUpdate(self.scene);
             *  FreezeUpdating = false;
             */
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
        cur.GotoNext(MoveType.After, ins => ins.MatchCallvirt<Scene>("BeforeUpdate"));
        {
            /*
             *  ExtraTimeActive += Engine.DeltaTime;
             */
            cur.EmitCall(typeof(Engine).GetProperty(nameof(Engine.DeltaTime)).GetGetMethod());
            cur.EmitLdsfld(ExtraTimeActiveField);
            cur.EmitAdd();
            cur.EmitStsfld(ExtraTimeActiveField);
        }
    }

    private static void FreezeUpdate(Scene scene)
    {
        // we need a better check method
        static bool IsSafeToUpdate(Entity entity)
            => VanillaModule.IsSafeToUpdate(entity) || HelpersModule.IsSafeToUpdate(entity);

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
            case DreamBlock dreamBlock:
            {
                BaseDreamBlockUpdate(dreamBlock);
                break;
            }
            }
        }
        if (scene is Level level)
        {
            level.WindSineTimer += Engine.DeltaTime;
            level.WindSine = (float)(Math.Sin(level.WindSineTimer) + 1.0) / 2f;
        }
        return;
    }

    public static bool OnExtraInterval(Scene _, float interval)
        => (int)((ExtraTimeActive - (double)Engine.DeltaTime) / (double)interval) < (int)(ExtraTimeActive / (double)interval);

    [MonoModLinkTo("Celeste.DreamBlock", "System.Void Update()"), MonoModForceCall]
    public static void BaseDreamBlockUpdate(DreamBlock dreamBlock) { throw null; }
}