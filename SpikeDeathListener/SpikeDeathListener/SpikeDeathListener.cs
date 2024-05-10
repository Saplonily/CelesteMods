using Celeste.Mod.Entities;

namespace Celeste.Mod.SpikeDeathListener;

[CustomEntity("SpikeDeathListener/SpikeDeathListener"), Tracked]
public sealed class SpikeDeathListener : Entity
{
    public string Flag;

    public SpikeDeathListener(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Flag = data.Attr("flag");
    }
}
