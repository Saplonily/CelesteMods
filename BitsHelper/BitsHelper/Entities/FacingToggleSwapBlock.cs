using Celeste.Mod.BitsHelper;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper;

[CustomEntity("BitsHelper/FacingToggleSwapBlock"), Tracked]
public partial class FacingToggleSwapBlock : Solid
{
    private class PathRenderer : Entity
    {
        private readonly FacingToggleSwapBlock block;
        private float timer;

        public PathRenderer(FacingToggleSwapBlock block) : base(block.Position)
        {
            this.block = block;
            Depth = 8999;
            timer = Calc.Random.NextFloat();
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime * 4f;
        }

        public override void Render()
        {
            float num = 0.5f * (0.5f + ((MathF.Sin(timer) + 1f) * 0.25f));
            DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * num);
        }
    }

    public Vector2 Direction;
    public bool Swapping;
    private Vector2 start;
    private Vector2 end;
    private float lerp;
    internal int target;
    private Rectangle moveRect;
    private float speed;
    private float redAlpha;
    private float particlesRemainder;
    private DisplacementRenderer.Burst burst;
    private readonly float speedRatio;
    private readonly float maxForwardSpeed;
    private readonly MTexture[,] nineSliceGreen;
    private readonly MTexture[,] nineSliceRed;
    private readonly MTexture[,] nineSliceTarget;
    private readonly Sprite middleGreen;
    private readonly Sprite middleRed;
    private readonly bool flip;

    public FacingToggleSwapBlock(Vector2 position, float width, float height, Vector2 node, bool flip, float moveSpeed)
        : base(position, width, height, false)
    {
        redAlpha = 1f;
        start = Position;
        end = node;
        maxForwardSpeed = moveSpeed / Vector2.Distance(start, end);
        speedRatio = moveSpeed / 360f;
        Direction.X = Math.Sign(end.X - start.X);
        Direction.Y = Math.Sign(end.Y - start.Y);
        this.flip = flip;
        int mx = (int)MathHelper.Min(X, node.X);
        int my = (int)MathHelper.Min(Y, node.Y);
        int max = (int)MathHelper.Max(X + Width, node.X + Width);
        int may = (int)MathHelper.Max(Y + Height, node.Y + Height);
        moveRect = new Rectangle(mx, my, max - mx, may - my);

        MTexture texBlock = GFX.Game["objects/BitsHelper/facingSwap/block"];
        MTexture texBlockRed = GFX.Game["objects/BitsHelper/facingSwap/blockRed"];
        MTexture texTarget = GFX.Game["objects/swapblock/moon/target"];

        nineSliceGreen = new MTexture[3, 3];
        nineSliceRed = new MTexture[3, 3];
        nineSliceTarget = new MTexture[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceGreen[i, j] = texBlock.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceRed[i, j] = texBlockRed.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceTarget[i, j] = texTarget.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        var bank = BitsHelperModule.Instance.SpriteBank;
        Add(middleGreen = bank.Create("facingToggleSwapBlockLight"));
        Add(middleRed = bank.Create("facingToggleSwapBlockLightRed"));
        Add(new LightOcclude(0.2f));
        Depth = -9999;
    }

    public FacingToggleSwapBlock(EntityData data, Vector2 offset)
        : this(
              data.Position + offset,
              data.Width, data.Height,
              data.Nodes[0] + offset,
              data.Bool("flip", false),
              data.Float("moveSpeed", 360f)
              )
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(new PathRenderer(this));
        var player = scene.Tracker.GetEntity<Player>();
        if (player is not null)
            UpdateWithFacing(player.Facing);
    }

    /*
    private void OnDash(Vector2 _)
    {
        Swapping = lerp < 1f;
        target = 1;
        returnTimer = ReturnTime;
        burst = (Scene as Level).Displacement.AddBurst(base.Center, 0.2f, 0f, 16f, 1f, null, null);
        speed = lerp >= 0.2f ? maxForwardSpeed : MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
        Audio.Stop(returnSfx, true);
        Audio.Stop(moveSfx, true);
        if (!Swapping)
        {
            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
            return;
        }
        moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", base.Center);
    }
    */

    public override void Update()
    {
        base.Update();
        /*
        if (returnTimer > 0f)
        {
            returnTimer -= Engine.DeltaTime;
            if (returnTimer <= 0f)
            {
                target = 0;
                speed = 0f;
                returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", base.Center);
            }
        }
        */
        if (burst != null)
            burst.Position = Center;

        redAlpha = Calc.Approach(redAlpha, ((target == 1) ? 0 : 1), Engine.DeltaTime * 32f);
        if (target == 0 && lerp == 0f)
        {
            middleRed.SetAnimationFrame(0);
            middleGreen.SetAnimationFrame(0);
        }
        speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);

        float pLerp = lerp;
        lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
        if (lerp != pLerp)
        {
            Vector2 vector = (end - start) * speed;
            Vector2 position = Position;
            if (target == 1)
            {
                vector = (end - start) * maxForwardSpeed;
                if (Scene.OnInterval(0.02f))
                    MoveParticles(end - start);
            }
            if (lerp < pLerp)
            {
                vector *= -1f;
            }
            MoveTo(Vector2.Lerp(start, end, lerp), vector);
            if (position != Position)
            {
                //Audio.Position(moveSfx, Center);
                //Audio.Position(returnSfx, Center);
                if (Position == start && target == 0)
                {
                    //Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                }
                else if (Position == end && target == 1)
                {
                    Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                }
            }
        }
        if (Swapping && lerp >= 1f)
            Swapping = false;
        if (lerp is 1f or 0f)
            speed = 0f;
        StopPlayerRunIntoAnimation = lerp is <= 0f or >= 1f;
    }

    public void UpdateWithFacing(Facings facing)
    {
        int p = target;
        target = (facing is Facings.Left ^ flip) ? 0 : 1;
        if (p != target)
            speed = 0f;
        if (lerp is 0f or 1f && target != lerp)
            burst = SceneAs<Level>().Displacement.AddBurst(Center, 0.2f, 0f, 8f);
    }

    private void MoveParticles(Vector2 normal)
    {
        Vector2 position;
        Vector2 range;
        float direction;
        float amountF;
        if (normal.X > 0f)
        {
            position = CenterLeft;
            range = Vector2.UnitY * (Height - 6f);
            direction = MathHelper.Pi;
            amountF = Math.Max(2f, Height / 14f);
        }
        else if (normal.X < 0f)
        {
            position = CenterRight;
            range = Vector2.UnitY * (Height - 6f);
            direction = 0f;
            amountF = Math.Max(2f, Height / 14f);
        }
        else if (normal.Y > 0f)
        {
            position = TopCenter;
            range = Vector2.UnitX * (Width - 6f);
            direction = -MathHelper.Pi / 2f;
            amountF = Math.Max(2f, Width / 14f);
        }
        else
        {
            position = BottomCenter;
            range = Vector2.UnitX * (Width - 6f);
            direction = MathHelper.Pi / 2;
            amountF = Math.Max(2f, Width / 14f);
        }
        particlesRemainder += amountF;
        int amount = (int)particlesRemainder;
        particlesRemainder -= amount;
        range *= 0.5f;
        SceneAs<Level>().Particles.Emit(SwapBlock.P_Move, amount, position, range, direction);
    }

    public override void Render()
    {
        Vector2 renderPosition = Position + Shake;
        if (lerp != target && speed > 0f)
        {
            Vector2 direction = (end - start).SafeNormalize();
            if (target == 1)
                direction *= -1f;

            float trailLength = (speed / maxForwardSpeed) * 16f * speedRatio;
            int trail = 2;
            while (trail < trailLength)
            {
                DrawBlockStyle(renderPosition + direction * trail, Width, Height, nineSliceGreen, middleGreen, Color.White * (1f - trail / trailLength));
                trail += 2;
            }
        }
        if (redAlpha < 1f)
            DrawBlockStyle(renderPosition, Width, Height, nineSliceGreen, middleGreen, Color.White);
        if (redAlpha > 0f)
            DrawBlockStyle(renderPosition, Width, Height, nineSliceRed, middleRed, Color.White * redAlpha);
    }

    private static void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] nineSlice, Sprite middle, Color color)
    {
        int gridWidth = (int)(width / 8f);
        int gridHeight = (int)(height / 8f);
        nineSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        nineSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        nineSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        nineSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
        for (int x = 1; x < gridWidth - 1; x++)
        {
            nineSlice[1, 0].Draw(pos + new Vector2(x * 8, 0f), Vector2.Zero, color);
            nineSlice[1, 2].Draw(pos + new Vector2(x * 8, height - 8f), Vector2.Zero, color);
        }
        for (int y = 1; y < gridHeight - 1; y++)
        {
            nineSlice[0, 1].Draw(pos + new Vector2(0f, y * 8), Vector2.Zero, color);
            nineSlice[2, 1].Draw(pos + new Vector2(width - 8f, y * 8), Vector2.Zero, color);
        }
        for (int x = 1; x < gridWidth - 1; x++)
            for (int y = 1; y < gridHeight - 1; y++)
                nineSlice[1, 1].Draw(pos + new Vector2(x, y) * 8f, Vector2.Zero, color);
        if (middle != null)
        {
            middle.Color = color;
            middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
            middle.Render();
        }
    }
}
