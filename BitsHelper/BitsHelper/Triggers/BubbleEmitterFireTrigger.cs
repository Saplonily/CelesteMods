using Celeste.Mod.BitsHelper.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

[CustomEntity("BitsHelper/BubbleEmitterFireTrigger")]
public sealed class BubbleEmitterFireTrigger : Trigger
{
    private readonly string flag;
    private readonly bool once;

    public BubbleEmitterFireTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        flag = data.Attr("flag");
        once = data.Bool("once");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        foreach (TriggerFloatingBubbleEmitter emitter in Scene.Tracker.GetEntities<TriggerFloatingBubbleEmitter>())
        {
            if (flag == "" || flag == emitter.Flag)
                emitter.Fire();
        }
        if (once)
            RemoveSelf();
    }
}