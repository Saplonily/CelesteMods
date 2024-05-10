using System.Reflection;
using MonoMod.Utils;

namespace Celeste.Mod.DeadableJellyFish;

public class DeadableJellyFishModule : EverestModule
{
    private static MethodInfo TrySquishWiggleMethod =
        typeof(Actor).GetMethod("TrySquishWiggle", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(CollisionData)], null);

    private static Color color = new Color(99, 165, 255);

    public override void Load()
    {
        On.Celeste.Glider.OnSquish += Glider_OnSquish;
    }

    private void Glider_OnSquish(On.Celeste.Glider.orig_OnSquish orig, Glider self, CollisionData data)
    {
        if (!(bool)TrySquishWiggleMethod.Invoke(self, [data]))
        {
            Entity entity = new(self.Position);
            DeathEffect deathEffect = new(color, new Vector2?(self.Center - self.Position));
            deathEffect.OnEnd = entity.RemoveSelf;
            entity.Add(deathEffect);
            self.Scene.Add(entity);
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", self.Position);
            self.RemoveSelf();
        }
    }

    public override void Unload()
    {
        On.Celeste.Glider.OnSquish -= Glider_OnSquish;
    }
}