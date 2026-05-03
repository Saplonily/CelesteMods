namespace Celeste.Mod.BitsHelper;

[Tracked]
public sealed class AlterEgoBoopable : Component
{
    public AlterEgoBoopable()
        : base(true, false)
    {
    }

    public void CheckAndAct(Player current)
    {
        var other = (Player)Entity;
        if (!current.CollideCheck(other))
            return;

        var m = current.StateMachine;
        if (
            m.State is Player.StNormal &&
            current.Speed.Y > 0f && current.Bottom <= other.Top + 3f
        )
        {
            Dust.Burst(current.BottomCenter, -MathF.PI / 2f, 8);
            (Scene as Level)?.DirectionalShake(Vector2.UnitY, 0.05f);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            current.Bounce(other.Top + 2f);
            current.Play(SFX.game_gen_thing_booped);
        }
        else if (
            m.State is not Player.StDash and not Player.StRedDash and not Player.StDreamDash and not Player.StBirdDashTutorial &&
            current.Speed.Y <= 0f && other.Bottom <= current.Top + 5f
        )
        {
            current.Speed.Y = Math.Max(current.Speed.Y, 16f);
        }
    }
}
