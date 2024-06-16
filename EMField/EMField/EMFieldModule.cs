using Monocle;

namespace Celeste.Mod.EMField;

public class EMFieldModule : EverestModule
{
    public override void Load()
    {
        On.Celeste.Player.Update += Player_Update;
        IL.Celeste.Player.NormalUpdate += Player_NormalUpdate;
    }

    public override void Unload()
    {
        On.Celeste.Player.Update -= Player_Update;
        IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        float q = 1f;
        float m = 1f;
        Vector2 center = self.Center;
        Vector2 acc = Vector2.Zero;

        var efields = self.Scene.Tracker.GetEntities<EField>();
        foreach (var entity in efields)
        {
            EField field = (EField)entity;
            Vector2 e = field.GetIntensityAt(center);
            Vector2 f = e * q;
            acc += f / m;
        }

        var mfields = self.Scene.Tracker.GetEntities<MField>();
        foreach (var entity in mfields)
        {
            MField field = (MField)entity;
            float b = field.GetIntensityAt(center) * 0.005f;
            Vector2 v = self.Speed;
            float intensity = q * v.Length() * b;
            Vector2 dir = v.Rotate((b > 0 ? -1f : 1f) * MathF.PI / 2).SafeNormalize();
            Vector2 f = intensity * dir;
            acc += f / m;
        }

        if (self.onGround)
            acc.Y = 0f;
        if (self.StateMachine.State == Player.StClimb)
            acc.X = 0f;
        self.Speed += acc * 60f * Engine.DeltaTime;
    }

    private static void Player_NormalUpdate(ILContext il)
    {
        ILCursor cur = new(il);
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(900f)))
        {
            cur.EmitLdarg0();
            cur.EmitDelegate((float v, Player player) => player.CollideFirst<IgnoreForceField>()?.Gravity is true ? 0f : v);
        }
        cur.Index = 0;
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(0.65f)))
        {
            cur.EmitLdarg0();
            cur.EmitDelegate((float v, Player player) => player.CollideFirst<IgnoreForceField>()?.Resistance is true ? 0f : v);
        }
    }
}