using Celeste.Mod.MaxHelpingHand.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

public sealed partial class FloatingBubble : Actor
{
    private Vector2 velocity;
    private float noFloatTimer;
    private float springCooldownTimer;
    private readonly Sprite sprite;
    private bool blowingFromPlayer;

    private bool broken = false;

    private readonly float speedMultiplier;

    public FloatingBubble(Vector2 position, Vector2 velocity, float speedMultiplier = 1.0f, bool fromPlayer = false) : base(position)
    {
        this.velocity = velocity;
        this.speedMultiplier = speedMultiplier;
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
        Vector2 actualSpeed = velocity;
        if (blowingFromPlayer && !CollideCheck<Player>())
            blowingFromPlayer = false;
        if (springCooldownTimer >= 0f)
            springCooldownTimer -= Engine.DeltaTime;
        if (noFloatTimer >= 0f)
            noFloatTimer -= Engine.DeltaTime;
        else
            actualSpeed += new Vector2(0, -60f) * speedMultiplier;
        Position += actualSpeed * Engine.DeltaTime;
        velocity.X = Calc.Approach(velocity.X, 0, 40f * speedMultiplier * Engine.DeltaTime);
        velocity.Y = Calc.Approach(velocity.Y, -60f * speedMultiplier, 20f * speedMultiplier * Engine.DeltaTime);
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
                collider.OnCollide(this);
        }
        if (sprite.CurrentAnimationID == "pop" && sprite.CurrentAnimationFrame == 1 && broken == false)
        {
            Collidable = false;
            Vector2 position = Position + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
            SceneAs<Level>().ParticlesFG.Emit(Player.P_CassetteFly, 10, position, new Vector2(8, 8), Color.White, 0);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            Audio.Play(BitsHelperSFX.BubbleTouch, Position);
            broken = true;
        }
    }

    public void OnSpring(Spring spring)
    {
        if (springCooldownTimer > 0)
            return;
        springCooldownTimer = 0.1f / speedMultiplier;
        switch (spring.Orientation)
        {
        case Spring.Orientations.WallLeft:
            MoveTowardsY(spring.CenterY + 5f, 4f);
            velocity.X = 160f * speedMultiplier;
            velocity.Y = -80f * speedMultiplier;
            noFloatTimer = 0.1f;
            break;
        case Spring.Orientations.WallRight:
            MoveTowardsY(spring.CenterY + 5f, 4f);
            velocity.X = -160f * speedMultiplier;
            velocity.Y = -80f * speedMultiplier;
            noFloatTimer = 0.1f;
            break;
        case Spring.Orientations.Floor:
            velocity.X *= 0.5f * speedMultiplier;
            velocity.Y = -160f * speedMultiplier;
            noFloatTimer = 0.15f;
            springCooldownTimer += 0.2f;
            break;
        }
        spring.BounceAnimate();
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