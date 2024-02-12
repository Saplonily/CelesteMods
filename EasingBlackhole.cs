using Celeste.Mod.Entities;

namespace Celeste.Mod.CNY2024Helper;

[CustomEntity("CNY2024Helper/EasingBlackhole")]
public sealed class EasingBlackhole : Entity
{
    private readonly float duration;
    private readonly float delay;
    private readonly float rotationSpeedA;
    private readonly float rotationSpeedB;
    private readonly float scaleA;
    private readonly float scaleB;
    private readonly Sprite sprite;
    private bool triggered;

    public EasingBlackhole(EntityData data, Vector2 offset)
        : this(
              data.Position + offset, new(data.Width, data.Height),
              data.Float("duration", 1f), data.Float("delay", 0f),
              data.Float("rotationSpeedA", 1f), data.Float("rotationSpeedB", 2f),
              data.Float("scaleA", 1f), data.Float("scaleB", 2f)
              )
    {

    }

    public EasingBlackhole(
        Vector2 position, Vector2 size,
        float duration, float delay,
        float rotationSpeedA, float rotationSpeedB,
        float scaleA, float scaleB
        )
        : base(position)
    {
        this.duration = duration;
        this.delay = delay;
        this.rotationSpeedA = rotationSpeedA;
        this.rotationSpeedB = rotationSpeedB;
        this.scaleA = scaleA;
        this.scaleB = scaleB;
        sprite = GFX.SpriteBank.Create("cny2024_GDDNblackhole");
        Collider = new Hitbox(size.X, size.Y);
        Add(sprite);
        sprite.Position = size / 2;
        sprite.CenterOrigin();
        sprite.Color.A = 0;
        sprite.Scale = new(scaleA);
        sprite.Rate = rotationSpeedA;
        sprite.Play("idle");
        Add(new PlayerCollider(OnCollidePlayer));
    }

    private void OnCollidePlayer(Player player)
    {
        if (triggered) return;
        triggered = true;
        Alarm.Set(this, delay, () =>
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineOut, t =>
            {
                sprite.Scale = new(Calc.LerpClamp(scaleA, scaleB, t.Eased));
                sprite.Color.A = (byte)(255f * t.Eased);
                sprite.Rate = Calc.LerpClamp(rotationSpeedA, rotationSpeedB, t.Eased);
            });
        });
    }
}