using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/TimerFloatingBubbleEmitter"), Tracked]
public sealed class TimerFloatingBubbleEmitter : FloatingBubbleEmitter
{
    private readonly float interval;
    private float timer;

    public TimerFloatingBubbleEmitter(Vector2 position, bool attach, float interval, float initialTimer)
        : base(position, attach)
    {
        this.interval = interval;
        timer = initialTimer;
    }

    public TimerFloatingBubbleEmitter(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("attach", false), data.Float("interval", 1f), data.Float("initialTimer", 1f))
    {
    }

    public override void Update()
    {
        base.Update();
        if (!firing)
        {
            timer -= Engine.DeltaTime;
            if (timer <= 0f)
            {
                timer = interval;
                Fire();
            }
        }
    }
}