using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/AlterEgoBlockField"), Tracked]
public sealed class AlterEgoBlockField : Entity
{
    private readonly bool blockIn, blockOut;

    public bool BlockIn => blockIn;

    public bool BlockOut => blockOut;

    public AlterEgoBlockField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("blockIn", false), data.Bool("blockOut", false))
    {
    }

    public AlterEgoBlockField(Vector2 position, int width, int height, bool blockIn, bool blockOut)
        : base(position)
    {
        Collider = new Hitbox(width, height);
        this.blockIn = blockIn;
        this.blockOut = blockOut;
    }
}
