using Celeste.Mod.BitsHelper.Entities;

namespace Celeste.Mod.BitsHelper;

public sealed class BlowBubbleComponent : Component
{
    private readonly MTexture indicatorTexture;
    private int bubblesCount;

    public BlowBubbleComponent()
        : base(true,true)
    {
        indicatorTexture = GFX.Game["BitsHelper/blowBubbleIndicator"];
        bubblesCount = 0;
    }

    public void OnCollectRefill()
        => bubblesCount++;

    public override void Update()
    {
        if (!Input.Grab.Pressed) return;
        if (Entity is not Player player) return;

        if (bubblesCount < 1)
            return;

        bubblesCount -= 1;
        Input.Grab.ConsumePress();
        Vector2 position = player.Position + new Vector2(0f, -8f);
        Vector2 speed = Vector2.UnitX * (float)player.Facing * 60f;
        if (Input.MoveY.Value == 1)
            speed.X = 0f;
        FloatingBubble bubble = new(position, speed, true);
        Scene.Add(bubble);
        Audio.Play(BitsHelperSFX.BubbleAppear, bubble.Position);
    }

    public override void Render()
    {
        if (Entity is not Player player) return;

        const int countPerLine = 5;

        int countReal = bubblesCount;
        int count = Math.Min(20 * countPerLine, countReal);

        int lines = 1 + (count - 1) / countPerLine;

        for (int curLine = 0; curLine < lines; curLine++)
        {
            int countOnLine = Math.Min(count, countPerLine);
            int totalWidth = countOnLine * 6 - 2;
            for (int i = 0; i < countOnLine; i++)
            {
                Vector2 ppos = player.Position;
                player.Position = player.ExactPosition;
                Vector2 position = player.Center + new Vector2(-totalWidth / 2 + i * 6, -15f - curLine * 6);
                player.Position = ppos;
                indicatorTexture.DrawJustified(position.Round(), new Vector2(0f, 0.5f));
            }
            count -= countOnLine;
        }
    }
}
