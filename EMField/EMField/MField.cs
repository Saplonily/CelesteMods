namespace Celeste.Mod.EMField;

[Tracked(true)]
public abstract class MField : Entity
{
    public abstract float GetIntensityAt(Vector2 position);

    public MField() : base()
    {
    }

    public MField(Vector2 position) : base(position)
    {
    }
}
