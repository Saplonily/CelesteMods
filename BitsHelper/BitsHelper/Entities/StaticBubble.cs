using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/StaticBubble")]
public sealed class StaticBubble : Entity
{
    private readonly Wiggler wiggler;

    private readonly bool oneUse;
    private readonly float respawnTime;
    private readonly Sprite sprite;

    private float respawnTimer;
    private bool broken = false;

    public StaticBubble(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("oneUse", true), data.Float("respawnTime", 2.5f))
    {
    }

    public StaticBubble(Vector2 position, bool oneUse, float respawnTime)
        : base(position)
    {
        this.oneUse = oneUse;
        this.respawnTime = respawnTime;
        respawnTimer = this.respawnTime;
        Collider = new Hitbox(14, 14, -7, -7);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = BitsHelperModule.Instance.SpriteBank.Create("bubble"));
        sprite.Play("static_idle");
        sprite.OnFinish = OnAnimationFinished;
        sprite.CenterOrigin();

        Add(wiggler = Wiggler.Create(0.25f, 4f, f => sprite.Scale = Vector2.One * (1f + f * 0.12f)));
    }

    public override void Update()
    {
        base.Update();
        if (sprite.CurrentAnimationID == "pop" && sprite.CurrentAnimationFrame == 1 && broken == false)
        {
            Collidable = false;
            Vector2 position = Position + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
            SceneAs<Level>().ParticlesFG.Emit(Player.P_CassetteFly, 10, position, new Vector2(8, 8), Color.White, 0);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            Audio.Play(BitsHelperSFX.BubbleTouch, Position);
            broken = true;
        }
        else if (broken && !oneUse)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Collidable = true;
                sprite.Play("respawn");
                wiggler.Start();
                broken = false;
                respawnTimer = respawnTime;
            }
        }
    }

    public void Burst()
    {
        sprite.Play("pop");
    }

    public void OnPlayer(Player player)
    {
        player.SuperBounce(Top);
        Burst();
    }

    public void OnAnimationFinished(string id)
    {
        if (id == "pop" && oneUse)
        {
            Remove(sprite);
            RemoveSelf();
        }
    }
}
