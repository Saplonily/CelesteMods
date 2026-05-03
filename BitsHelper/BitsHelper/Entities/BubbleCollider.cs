using Celeste.Mod.BitsHelper.Entities;

namespace Celeste.Mod.BitsHelper;

[Tracked]
public sealed partial class BubbleCollider : Component
{
    private readonly Collider collider;
    private readonly Action<FloatingBubble> onBubble;

    public BubbleCollider(Action<FloatingBubble> onBubble = null, Collider collider = null)
        : base(true, false)
    {
        this.collider = collider;
        this.onBubble = onBubble;
    }

    public bool Check(FloatingBubble bubble)
    {
        Collider collider = Entity.Collider;
        if (this.collider != null)
        {
            Entity.Collider = this.collider;
        }
        bool result = bubble.CollideCheck(Entity);
        Entity.Collider = collider;
        return result;
    }

    public void OnCollide(FloatingBubble bubble)
        => onBubble(bubble);
}