using Celeste.Mod.BitsHelper.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

[CustomEntity("BitsHelper/AlterEgoConfigTrigger")]
public sealed class AlterEgoConfigTrigger : Trigger
{
    private readonly bool hold, boop, keySwitching;

    public AlterEgoConfigTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        hold = data.Bool("holdInteractions", true);
        boop = data.Bool("boopInteractions", true);
        keySwitching = data.Bool("keySwitching", true);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var controller = AlterEgoController.GetOrCreate(player.Scene, player);
        controller.SetInteractions(hold, boop);
        controller.KeySwitching = keySwitching;
    }
}
