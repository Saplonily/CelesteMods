using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/TimerFloatingBubbleEmitter"), Tracked]
public sealed class TimerFloatingBubbleEmitter : FloatingBubbleEmitter
{
    private readonly float interval;
    private float timer;

    public TimerFloatingBubbleEmitter(Vector2 position, bool attach, float interval, float initialTimer, float speedMultiplier, bool noSfx)
        : base(position, attach, speedMultiplier, noSfx)
    {
        this.interval = interval;
        timer = initialTimer;
    }

    public TimerFloatingBubbleEmitter(EntityData data, Vector2 offset)
        : this(
              data.Position + offset,
              data.Bool("attach", false),
              data.Float("interval", 1f),
              data.Float("initialTimer", 1f),
              data.Float("speedMultiplier", 1.0f),
              data.Bool("noSfx", false)
          )
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