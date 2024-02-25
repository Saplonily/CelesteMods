using System.Runtime.CompilerServices;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod;

namespace Celeste.Mod.BetterFreezeFrames;

public class BetterFreezeFramesModule : EverestModule
{
    private static bool freezeUpdating;

    public override void Load()
    {
        IL.Monocle.Engine.Update += Engine_Update;
        On.Celeste.Level.Update += Level_Update;
        On.Celeste.ScreenWipe.Update += ScreenWipe_Update;
    }

    public override void Unload()
    {
        IL.Monocle.Engine.Update -= Engine_Update;
        On.Celeste.Level.Update -= Level_Update;
        On.Celeste.ScreenWipe.Update -= ScreenWipe_Update;
    }

    private void ScreenWipe_Update(On.Celeste.ScreenWipe.orig_Update orig, ScreenWipe self, Scene scene)
    {
        if (scene is Level)
        {
            if (!freezeUpdating)
                orig(self, scene);
        }
        else
            orig(self, scene);
    }

    private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if (Input.MenuJournal.Check)
        {
            //Celeste.Freeze(0.5f);
        }
    }

    private void Engine_Update(ILContext il)
    {
        ILCursor cur = new(il);
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchStsfld("Monocle.Engine", "FreezeTimer")))
        {
            cur.EmitDelegate<Action>(() => freezeUpdating = true);
            cur.Emit(OpCodes.Ldarg_0);
            cur.Emit(OpCodes.Ldfld, typeof(Engine).GetField("scene", BindingFlags.NonPublic | BindingFlags.Instance));
            cur.Emit(OpCodes.Dup);
            cur.Emit(OpCodes.Call, typeof(Scene).GetProperty("RendererList").GetGetMethod());
            cur.Emit(OpCodes.Call, typeof(RendererList).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance));
            cur.EmitDelegate((Scene scene) =>
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
                        //or DreamBlock  (FIXME: the movement of dreamblock is impled in Tween component)
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
                        DynamicData data = DynamicData.For(refill);
                        float orig = data.Get<float>("respawnTimer");
                        data.Set("respawnTimer", 100f);
                        refill.Update();
                        data.Set("respawnTimer", orig);
                        break;
                    }
                    }
                }
                return;
            });
            cur.EmitDelegate<Action>(() => freezeUpdating = false);
        }
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    private static void ComponentOnlyUpdate(Entity entity) { }
}