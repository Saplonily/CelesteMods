using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Entities;

[CustomEntity("BitsHelper/BubbleRefill")]
public sealed class BubbleRefill : Entity
{
    private readonly Sprite sprite;
    private readonly Sprite flash;
    private readonly Image outline;
    private readonly Wiggler wiggler;
    private readonly BloomPoint bloom;
    private readonly VertexLight light;
    private readonly SineWave sine;
    private readonly bool oneUse;

    private Level level;
    private float respawnTimer;

    public BubbleRefill(Vector2 position, bool oneUse)
        : base(position)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
        this.oneUse = oneUse;

        Add(outline = new Image(GFX.Game["objects/BitsHelper/bubbleRefill/outline0"]));
        outline.CenterOrigin();
        outline.Visible = false;

        Add(sprite = new Sprite(GFX.Game, "objects/BitsHelper/bubbleRefill/idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle", false, false);
        sprite.CenterOrigin();

        Add(flash = new Sprite(GFX.Game, "objects/BitsHelper/bubbleRefill/flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = _ => flash.Visible = false;
        flash.CenterOrigin();

        Add(wiggler = Wiggler.Create(1f, 4f, v => sprite.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f), false, false));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.5f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
    }

    public BubbleRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("oneUse", false))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
                Respawn();
        }
        else if (Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(Refill.P_Glow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.5f;
        if (Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", true, false);
            flash.Visible = true;
        }
    }

    private void Respawn()
    {
        if (Collidable)
            return;
        Collidable = true;
        sprite.Visible = true;
        outline.Visible = false;
        Depth = -100;
        wiggler.Start();
        Audio.Play("event:/game/general/diamond_return", Position);
        level.ParticlesFG.Emit(Refill.P_Regen, 16, Position, Vector2.One * 2f);
    }

    private void UpdateY()
        => flash.Y = sprite.Y = bloom.Y = sine.Value * 2f;

    public override void Render()
    {
        if (sprite.Visible)
            sprite.DrawOutline(1);
        base.Render();
    }

    private void OnPlayer(Player player)
    {
        BitsHelperModule.Session.BlowBubbleCount++;
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(RefillRoutine(player), true));
        respawnTimer = 2.5f;
    }

    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake(0.3f);
        sprite.Visible = (flash.Visible = false);
        if (!oneUse)
            outline.Visible = true;
        Depth = 8999;
        yield return 0.05f;
        float angle = player.Speed.Angle();
        level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle - MathHelper.PiOver2);
        level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle + MathHelper.PiOver2);
        SlashFx.Burst(Position, angle);
        if (oneUse)
            RemoveSelf();
        yield break;
    }
}