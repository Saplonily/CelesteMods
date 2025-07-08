namespace Celeste.Mod.BetterFreezeFrames;

public static class VanillaModule
{
    public static void Load()
    {
        On.Monocle.Coroutine.Update += Coroutine_Update;
        On.Celeste.ScreenWipe.Update += ScreenWipe_Update;
        On.Celeste.TriggerSpikes.Update += TriggerSpikes_Update;
        IL.Celeste.DreamBlock.Update += HookUtils.ILHookSkipBaseUpdate<Solid>;
        IL.Celeste.LightningRenderer.Update += HookUtils.ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update += HookUtils.ILHookReplaceOnInterval;
        IL.Celeste.Refill.Update += HookUtils.ILHookSkipBaseUpdate<Entity>;
    }

    public static void Unload()
    {
        On.Monocle.Coroutine.Update -= Coroutine_Update;
        On.Celeste.ScreenWipe.Update -= ScreenWipe_Update;
        On.Celeste.TriggerSpikes.Update -= TriggerSpikes_Update;
        IL.Celeste.DreamBlock.Update -= HookUtils.ILHookSkipBaseUpdate<Solid>;
        IL.Celeste.LightningRenderer.Update -= HookUtils.ILHookReplaceOnInterval;
        IL.Celeste.SeekerBarrierRenderer.Update -= HookUtils.ILHookReplaceOnInterval;
        IL.Celeste.Refill.Update -= HookUtils.ILHookSkipBaseUpdate<Entity>;
    }

    private static void Coroutine_Update(On.Monocle.Coroutine.orig_Update orig, Coroutine self)
    {
        if (!Bff.FreezeUpdating)
            orig(self);
    }

    public static bool IsSafeToUpdate(Entity entity)
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
                  or FloatingDebris
                  or Decal
                  or TriggerSpikes // with extra patch
                  or ForegroundDebris
                  or Clothesline
                  or CliffFlags
                  or BigWaterfall
                  or MoonCreature
                  or Wire
                  or SolidTiles
                  ;

    private static void TriggerSpikes_Update(On.Celeste.TriggerSpikes.orig_Update orig, TriggerSpikes self)
    {
        if (Bff.FreezeUpdating)
            for (int i = 0; i < self.spikes.Length; i++)
                self.spikes[i].TentacleFrame += Engine.DeltaTime * 12f;
        else
            orig(self);
    }

    private static void ScreenWipe_Update(On.Celeste.ScreenWipe.orig_Update orig, ScreenWipe self, Scene scene)
    {
        if (scene is Level)
        {
            if (!Bff.FreezeUpdating)
                orig(self, scene);
        }
        else
        {
            orig(self, scene);
        }
    }
}
