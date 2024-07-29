using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/TriggerFloatingBubbleEmitter"), Tracked]
public sealed class TriggerFloatingBubbleEmitter : FloatingBubbleEmitter
{
    public string Flag;

    public TriggerFloatingBubbleEmitter(Vector2 position, bool attach, string flag)
        : base(position, attach)
    {
        Flag = flag;
    }

    public TriggerFloatingBubbleEmitter(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("attach", false), data.Attr("flag"))
    {
    }
}