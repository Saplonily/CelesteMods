using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/TriggerFloatingBubbleEmitter"), Tracked]
public sealed class TriggerFloatingBubbleEmitter : FloatingBubbleEmitter
{
    public string Flag;

    public TriggerFloatingBubbleEmitter(Vector2 position, bool attach, string flag, float speedMultiplier, bool noSfx)
        : base(position, attach, speedMultiplier, noSfx)
    {
        Flag = flag;
    }

    public TriggerFloatingBubbleEmitter(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("attach", false), data.Attr("flag"), data.Float("speedMultiplier", 1.0f), data.Bool("noSfx", false))
    {
    }
}