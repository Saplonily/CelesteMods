namespace Celeste.Mod.BitsHelper.Entities;

[Tracked]
public partial class BubbleCollider : Component
{
    private readonly Collider collider;

    public BubbleCollider(Collider collider = null) : base(true, false)
    {
        this.collider = collider;
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
}