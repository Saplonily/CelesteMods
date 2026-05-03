namespace Celeste.Mod.BitsHelper;

[TrackedAs(typeof(Holdable))]
public sealed class AlterEgoHoldable : Holdable
{
    public AlterEgoHoldable()
    {
        OnPickup = () =>
        {
            var holdingPlayer = (Player)Entity;
            holdingPlayer.StateMachine.State = Player.StFrozen;
            holdingPlayer.Speed = Vector2.Zero;
            holdingPlayer.Sprite.Play("idle");
        };

        OnRelease = f =>
        {
            var holdingPlayer = (Player)Entity;
            holdingPlayer.StateMachine.State = Player.StNormal;
            holdingPlayer.Speed = f * 296f;
        };

        OnCarry = p =>
        {
            var holdingPlayer = (Player)Entity;
            holdingPlayer.Position = Calc.Floor(p);
        };
    }
}
