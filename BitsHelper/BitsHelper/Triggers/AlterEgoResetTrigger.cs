using Celeste.Mod.BitsHelper.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

[CustomEntity("BitsHelper/AlterEgoResetTrigger")]
public sealed class AlterEgoResetTrigger : Trigger
{
    private readonly bool oneUse;

    public AlterEgoResetTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        oneUse = data.Bool("oneUse");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var c = AlterEgoController.Get(Scene);
        if (c is not null)
        {
            c.Reset();
            if (oneUse)
                RemoveSelf();
        }
    }
}
