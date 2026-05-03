using Celeste.Mod.BitsHelper.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

[CustomEntity("BitsHelper/AlterEgoRemoveTrigger")]
public sealed class AlterEgoRemoveTrigger : Trigger
{
    private readonly bool oneUse;
    private readonly string flag;

    public AlterEgoRemoveTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        oneUse = data.Bool("oneUse", true);
        flag = data.Attr("flag", string.Empty);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var controller = AlterEgoController.GetOrCreate(Scene, player);
        if (controller.TryRemove(player))
        {
            if (!string.IsNullOrEmpty(flag))
                SceneAs<Level>().Session.SetFlag(flag);
            if (oneUse)
                RemoveSelf();
        }
    }
}
