using Celeste.Mod.Entities;

namespace Celeste.Mod.EMField;

[CustomEntity("EMField/UniformRectangleMField")]
public class UniformRectangleMField : MField
{
    protected float intensity;

    public UniformRectangleMField(Vector2 position, float width, float height, float intensity)
        : base(position)
    {
        this.intensity = intensity;
        Collider = new Hitbox(width, height);
    }

    public UniformRectangleMField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Float("intensity", 0f))
    {

    }

    public override float GetIntensityAt(Vector2 position)
    {
        if (Collider.Collide(position))
            return intensity;
        return 0f;
    }

    public override void Render()
    {
        base.Render();
        Draw.HollowRect(Collider, intensity > 0f ? Color.AliceBlue : Color.Brown);
    }
}