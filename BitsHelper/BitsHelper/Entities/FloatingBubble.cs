using Celeste.Mod.MaxHelpingHand.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

public partial class FloatingBubble : Actor
{
    private Vector2 speed;
    private float noFloatTimer;
    private float springCooldownTimer;
    private readonly Sprite sprite;
    private bool blowingFromPlayer;

    private bool broken = false;

    public FloatingBubble(Vector2 position, Vector2 initialSpeed, bool fromPlayer = false) : base(position)
    {
        speed = initialSpeed;
        Collider = new Hitbox(14, 14, -7, -7);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = BitsHelperModule.Instance.SpriteBank.Create("bubble"));
        sprite.OnFinish = OnAnimationFinished;
        sprite.CenterOrigin();
        blowingFromPlayer = fromPlayer;
    }

    public override bool IsRiding(JumpThru jumpThru) => false;

    public override bool IsRiding(Solid solid) => false;

    public override void Update()
    {
        base.Update();
        Vector2 actualSpeed = speed;
        if (blowingFromPlayer && !CollideCheck<Player>())
            blowingFromPlayer = false;
        if (springCooldownTimer >= 0f)
            springCooldownTimer -= Engine.DeltaTime;
        if (noFloatTimer >= 0f)
            noFloatTimer -= Engine.DeltaTime;
        else
            actualSpeed += new Vector2(0, -60f);
        Position += actualSpeed * Engine.DeltaTime;
        speed.X = Calc.Approach(speed.X, 0, 40f * Engine.DeltaTime);
        speed.Y = Calc.Approach(speed.Y, -60f, 20f * Engine.DeltaTime);
        if (CollideCheck<Solid>())
            Burst();
        Rectangle levelBounds = SceneAs<Level>().Bounds;
        if ((Position.X > levelBounds.Right + 10 || Position.X < levelBounds.Left - 10) ||
            (Position.Y > levelBounds.Bottom + 10 || Position.Y < levelBounds.Top - 10))
        {
            Burst();
        }
        foreach (BubbleCollider collider in Scene.Tracker.GetComponents<BubbleCollider>())
        {
            if (collider.Check(this))
            {
                if (collider.Entity is Spring spring)
                {
                    if (springCooldownTimer <= 0)
                    {
                        HitSpring(spring);
                        spring.BounceAnimate();
                    }
                }
                else if (collider.Entity is TouchSwitch)
                {
                    (collider.Entity as TouchSwitch).TurnOn();
                }
                else if (collider.Entity is FlagTouchSwitch)
                {
                    (collider.Entity as FlagTouchSwitch).TurnOn();
                }
            }
        }
        if (sprite.CurrentAnimationID == "pop" && sprite.CurrentAnimationFrame == 1 && broken == false)
        {
            Collidable = false;
            Vector2 position = Position + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
            SceneAs<Level>().ParticlesFG.Emit(Player.P_CassetteFly, 10, position, new Vector2(8, 8), Color.White, 0);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            Audio.Play("event:/BitsHelper/bubblefx/bubble_touch", Position);
            broken = true;
        }
    }

    public bool HitSpring(Spring spring)
    {
        springCooldownTimer = 0.05f;
        switch (spring.Orientation)
        {
        case Spring.Orientations.WallLeft:
            MoveTowardsY(spring.CenterY + 5f, 4f);
            speed.X = 160f;
            speed.Y = -80f;
            noFloatTimer = 0.1f;
            break;
        case Spring.Orientations.WallRight:
            MoveTowardsY(spring.CenterY + 5f, 4f);
            speed.X = -160f;
            speed.Y = -80f;
            noFloatTimer = 0.1f;
            break;
        case Spring.Orientations.Floor:
            speed.X *= 0.5f;
            speed.Y = -160f;
            noFloatTimer = 0.15f;
            springCooldownTimer += 0.2f;
            break;
        }
        return true;
    }

    public void Burst()
    {
        sprite.Play("pop");
    }

    public void OnPlayer(Player player)
    {
        if (blowingFromPlayer) return;
        player.SuperBounce(Top);
        Burst();
    }

    public void OnAnimationFinished(string id)
    {
        Remove(sprite);
        RemoveSelf();
    }
}