namespace Celeste.Mod.EMField;

[Tracked(true)]
public abstract class EField : Entity
{
	public const float K = 300f;

    public abstract Vector2 GetIntensityAt(Vector2 position);

	public EField() : base()
	{
	}

	public EField(Vector2 position) : base(position)
	{
	}

    protected static Vector2 DataGetDirection(EntityData data)
        => (data.Nodes[0] - data.Position).SafeNormalize();
}