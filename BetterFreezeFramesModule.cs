using System.Reflection;

namespace Celeste.Mod.BetterFreezeFrames;

public class BetterFreezeFramesModule : EverestModule
{
    public static BetterFreezeFramesModule Instance { get; private set; }

    public override Type SettingsType => typeof(BetterFreezeFramesSettings);
    public static BetterFreezeFramesSettings Settings => (BetterFreezeFramesSettings)Instance._Settings;

    public static bool FreezeUpdating;
    public readonly static FieldInfo FreezeUpdatingField =
        typeof(BetterFreezeFramesModule).GetField(nameof(FreezeUpdating));
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
        IL.Monocle.Engine.Update += Engine_Update;
        On.Celeste.ScreenWipe.Update += ScreenWipe_Update;
        On.Celeste.DreamBlock.Update += DreamBlock_Update;
        On.Celeste.TriggerSpikes.Update += TriggerSpikes_Update;
        LoadedStuffs = true;
    }

    public override void Unload()
    {
        if (!LoadedStuffs) return;
#if DEBUG
        On.Celeste.Level.Update -= Level_Update;
#endif
        IL.Monocle.Engine.Update -= Engine_Update;
        On.Celeste.ScreenWipe.Update -= ScreenWipe_Update;
        On.Celeste.DreamBlock.Update -= DreamBlock_Update;
        On.Celeste.TriggerSpikes.Update -= TriggerSpikes_Update;
        LoadedStuffs = false;
    }

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
            if (!FreezeUpdating) orig(self, scene);
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

#if DEBUG
    private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if (Input.MenuJournal.Check)
        {
            Celeste.Freeze(0.5f);
        }
    }
#endif

    private void Engine_Update(ILContext il)
    {
        ILCursor cur = new(il);
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchStsfld("Monocle.Engine", "FreezeTimer")))
        {
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
                or DustEdges
                or TouchSwitch
                or WaterFall
                or LightBeam
                or LightningStrike
                or LightningRenderer // this is a little buggy
                or DreamBlock // not completely "safe"
                or FloatingDebris
                or Decal
                or TriggerSpikes // not completely "safe"
            )
            {
                entity.Update();
                continue;
            }
            // update "maybe safe" entities here
            switch (entity)
            {
            case Water water:
                foreach (Water.Surface surface in water.Surfaces)
                    surface.Update();
                break;
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
}