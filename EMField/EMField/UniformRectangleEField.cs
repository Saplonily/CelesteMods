using Celeste.Mod.Entities;

namespace Celeste.Mod.EMField;

[CustomEntity("EMField/UniformRectangleEField")]
public class UniformRectangleEField : EField
{
    protected Vector2 dir;
    protected float intensity;

    public UniformRectangleEField(Vector2 position, float width, float height, float intensity, Vector2 dir)
        : base(position)
    {
        this.intensity = intensity;
        this.dir = dir;
        Collider = new Hitbox(width, height);
    }

    public UniformRectangleEField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Float("intensity", 0f), DataGetDirection(data))
    {

    }

    public override Vector2 GetIntensityAt(Vector2 position)
    {
        if (Collider.Collide(position))
            return dir * intensity;
        return Vector2.Zero;
    }

    public override void Render()
    {
        base.Render();
        Draw.HollowRect(Collider, Color.AliceBlue);
        Draw.Line(Center, Center + dir * intensity, Color.Red);
    }
}