using Celeste.Mod.Entities;

namespace Celeste.Mod.EMField;

[CustomEntity("EMField/IgnoreForceField"), Tracked]
public sealed class IgnoreForceField : Entity
{
    public bool Gravity;
    public bool Resistance;

    public IgnoreForceField(Vector2 position, float width, float height, bool gravity, bool resistance)
        : base(position)
    {
        Collider = new Hitbox(width, height);
        Gravity = gravity;
        Resistance = resistance;
    }

    public IgnoreForceField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("gravity"), data.Bool("resistance"))
    {
    }
}
