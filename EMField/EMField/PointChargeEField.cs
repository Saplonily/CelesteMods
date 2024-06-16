using Celeste.Mod.Entities;

namespace Celeste.Mod.EMField;

[CustomEntity("EMField/PointChargeEField")]
public class PointChargeEField : EField
{
    protected float charge;

    public PointChargeEField(Vector2 position, float charge)
        : base(position)
    {
        this.charge = charge;
        Collider = new Circle(charge);
        Collidable = false;
    }

    public PointChargeEField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("charge", 0f))
    {

    }

    public override Vector2 GetIntensityAt(Vector2 position)
    {
        Vector2 center = Center;
        Vector2 diff = position - center;
        float distance2 = MathF.Pow(diff.LengthSquared(), 0.8f);
        if (distance2 < 16f) distance2 = 16f;
        Vector2 dir = diff.SafeNormalize();
        float e = charge / distance2;
        return K * dir * e;
    }

    public override void Render()
    {
        base.Render();
        Draw.Circle(Center, 2f, Color.Red, 4);
        Draw.Circle(Center, charge, Color.AliceBlue, 24);
    }
}