using Celeste.Mod.Entities;

namespace Celeste.Mod.RelativeMadeline;

[Tracked, TrackedAs(typeof(TimeRateModifier))]
public sealed class RelativeControlComponent : TimeRateModifier
{
    public RelativeControlComponent()
        : base(1f, true)
    {
        Active = true;
    }

    public override void Update()
    {
        base.Update();
        float threshold = RelativeMadelineModule.Settings.SpeedThreshold * 10f;
        float target = RelativeMadelineModule.Settings.SpeedTarget * 10f;
        Player player = EntityAs<Player>();
        float speed = player.Speed.Length();
        speed = Math.Max(threshold, speed);
        Multiplier = Math.Min(1.0f, target / speed);
    }
}
