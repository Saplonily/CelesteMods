using Celeste.Mod.BitsHelper.Entities;

namespace Celeste.Mod.BitsHelper;

public static class BlowBubble
{
    public static void Load()
    {
        On.Celeste.Player.Added += Player_Added;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.Render += Player_Render;
    }

    public static void Unload()
    {
        On.Celeste.Player.Added -= Player_Added;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.Render -= Player_Render;
    }

    private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        BitsHelperModule.Session.BlowBubbleCount = 0;
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        UpdateBlowBubble(self);
    }

    private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
    {
        orig(self);
        DrawBlowBubbleIndicator(self);
    }

    public static void UpdateBlowBubble(Player player)
    {
        if (!Input.Grab.Pressed) return;

        var session = BitsHelperModule.Session;
        if (session.BlowBubbleCount < 1) return;

        session.BlowBubbleCount -= 1;
        Input.Grab.ConsumePress();
        Vector2 position = player.Position + new Vector2(0f, -8f);
        Vector2 speed = Vector2.UnitX * (float)player.Facing * 60f;
        if (Input.MoveY.Value == 1)
            speed.X = 0f;
        FloatingBubble bubble = new(position, speed, true);
        player.Scene.Add(bubble);
        Audio.Play("event:/BitsHelper/bubblefx/bubble_appear", bubble.Position);
    }

    public static void DrawBlowBubbleIndicator(Player player)
    {
        const int countPerLine = 5;
        MTexture tex = BitsHelperModule.Instance.BlowBubbleIndicatorTexture;

        int countReal = BitsHelperModule.Session.BlowBubbleCount;
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
                tex.DrawJustified(position.Round(), new Vector2(0f, 0.5f));
            }
            count -= countOnLine;
        }
    }
}
