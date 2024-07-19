using Celeste.Mod.Entities;

namespace Celeste.Mod.ChroniaHelperIndicatorZone;

[CustomEntity(EntityStringId, EntityStringId2), Tracked]
public sealed class PlayerIndicatorZone : Entity
{
    public const string EntityStringId = "ChroniaHelper/PlayerIndicatorZone";
    public const string EntityStringId2 = "ChroniaHelper/PlayerIndicatorZoneCustom";

    public enum ZoneMode { Limited, Toggle, None }
    public enum FlagMode { None, Zone, Enable, Disable }

    public sealed class ZoneConfig
    {
        public ZoneMode ZoneMode;
        public string ControlFlag;
        public bool RenderBorder;
        public bool RenderInside;
        public bool RenderContinuousLine;
        public Color ZoneColor;
        public int Depth;
        public List<MTexture> Icons;
        public List<Vector2> IconOffsets;
        public List<Color> IconColors;
        public FlagMode FlagMode;
        public string Flag;

        public static ZoneConfig FromEntityData(EntityData data)
        {
            try
            {
                if (data.Has("iconPrefix"))
                    return FromNew(data);
                else if (data.Has("slotDirectories"))
                    return FromOldCustom(data);
                else if (data.Has("slot1Directory"))
                    return FromOldNonCustom(data);
                else
                    throw new Exception("Unrecognized EntityData version.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load entity data.", e);
            }
        }

        private static ZoneConfig FromOldNonCustom(EntityData data)
        {
            ZoneConfig config = new();
            ReadOldNonSlotParams(config, data);

            bool slot1 = data.Bool("slot1Enabled", true);
            bool slot2 = data.Bool("slot2Enabled", false);
            bool slot3 = data.Bool("slot3Enabled", false);

            List<MTexture> icons = new();
            List<Vector2> offsets = new();
            List<Color> colors = new();

            if (slot1)
            {
                icons.Add(GFX.Game[data.Attr("slot1Directory", "ChroniaHelper/PlayerIndicator/chevron")]);
                offsets.Add(new(-11f, 6f));
                colors.Add(data.HexColor("slot1Color", Color.White));
            }
            if (slot2)
            {
                icons.Add(GFX.Game[data.Attr("slot2Directory", "ChroniaHelper/PlayerIndicator/triangle")]);
                offsets.Add(new(0f, 6f));
                colors.Add(data.HexColor("slot2Color", Color.White));
            }
            if (slot3)
            {
                icons.Add(GFX.Game[data.Attr("slot3Directory", "ChroniaHelper/PlayerIndicator/square")]);
                offsets.Add(new(12f, 6f));
                colors.Add(data.HexColor("slot3Color", Color.White));
            }
            return config;
        }

        private static ZoneConfig FromOldCustom(EntityData data)
        {
            ZoneConfig config = new();
            ReadOldNonSlotParams(config, data);

            string slotDirectories = data.Attr(
                "slotDirectories",
                "ChroniaHelper/PlayerIndicator/chevron,ChroniaHelper/PlayerIndicator/triangle"
                );
            string iconOffsets = data.Attr("iconCoordinates", "0,6;-11,6");
            string iconColors = data.Attr("iconColors", "ffffff,ffffff");

            StringSplitOptions option = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            config.Icons = slotDirectories.Split(',', option).Select(s => GFX.Game[s]).ToList();
            config.IconOffsets = iconOffsets.Split(';', option).Select(ParseVector2).ToList();
            config.IconColors = iconColors.Split(',', option).Select(Calc.HexToColor).ToList();
            return config;
        }

        private static void ReadOldNonSlotParams(ZoneConfig config, EntityData data)
        {
            config.ZoneMode = data.Attr("mode") switch
            {
                "limited" => ZoneMode.Limited,
                "none" => ZoneMode.None,
                "toggle" => ZoneMode.Toggle,
                _ => ZoneMode.Limited
            };
            config.ControlFlag = data.Attr("zoneControlFlag", "");
            config.RenderBorder = data.Bool("renderBorder", false);
            config.RenderInside = data.Bool("renderInside", false);
            config.RenderContinuousLine = data.Bool("continuousLine", false);
            config.ZoneColor = data.HexColor("zoneColor", Color.White);
            config.Depth = data.Int("depth", -20000);
            config.FlagMode = data.Attr("flagToggle") switch
            {
                "enableFlag" => FlagMode.Enable,
                "disableFlag" => FlagMode.Disable,
                "inZone" => FlagMode.Zone,
                "disabled" => FlagMode.None,
                _ => FlagMode.None
            };
            config.Flag = data.Attr("flag", "flag");
        }

        private static ZoneConfig FromNew(EntityData data)
        {
            ZoneConfig config = new()
            {
                ZoneMode = (ZoneMode)data.Int("mode", 0),
                ControlFlag = data.Attr("controlFlag", ""),
                RenderBorder = data.Bool("renderBorder", false),
                RenderInside = data.Bool("renderInside", false),
                RenderContinuousLine = data.Bool("renderContinuousLine", false),
                ZoneColor = data.HexColor("zoneColor", Color.White),
                Depth = data.Int("depth", -20000),
                FlagMode = (FlagMode)data.Int("flagMode"),
                Flag = data.Attr("flag", "flag")
            };
            // TODO convert old data
            string iconPrefix = data.Attr("iconPrefix", "ChroniaHelper/PlayerIndicator/");
            string icons = data.Attr("icons", "chevron,triangle");
            string iconOffsets = data.Attr("iconOffsets", "0,6;-11,6");
            string iconColors = data.Attr("iconColors", "ffffff,ffffff");

            StringSplitOptions option = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            config.Icons = icons.Split(',', option).Select(s => GFX.Game[iconPrefix + s]).ToList();
            config.IconOffsets = iconOffsets.Split(';', option).Select(ParseVector2).ToList();
            config.IconColors = iconColors.Split(',', option).Select(Calc.HexToColor).ToList();
            return config;
        }

        private static Vector2 ParseVector2(string s)
        {
            int index = s.IndexOf(',');
            float part1 = float.Parse(s[..index]);
            float part2 = float.Parse(s[(index + 1)..]);
            return new Vector2(part1, part2);
        }
    }

    private readonly ZoneMode zoneMode;
    private readonly string controlFlag;
    private readonly bool renderBorder;
    private readonly bool renderInside;
    private readonly bool renderContinuousLine;
    private readonly Color zoneColor;
    private readonly List<MTexture> icons;
    private readonly List<Vector2> iconOffsets;
    private readonly List<Color> iconColors;
    private readonly FlagMode flagMode;
    private readonly string flag;

    private bool playerIn;
    private Player lastPlayer;

    public PlayerIndicatorZone(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, ZoneConfig.FromEntityData(data))
    {
    }

    public PlayerIndicatorZone(Vector2 position, int width, int height, ZoneConfig config)
        : base(position)
    {
        Collider = new Hitbox(width, height);

        zoneMode = config.ZoneMode;
        controlFlag = config.ControlFlag;
        renderBorder = config.RenderBorder;
        renderInside = config.RenderInside;
        renderContinuousLine = config.RenderContinuousLine;
        zoneColor = config.ZoneColor;
        Depth = config.Depth;
        flagMode = config.FlagMode;
        flag = config.Flag;
        icons = config.Icons;
        iconOffsets = config.IconOffsets;
        iconColors = config.IconColors;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        lastPlayer = null;
    }

    public override void Update()
    {
        base.Update();
        if (zoneMode is ZoneMode.None) return;

        Session session = SceneAs<Level>().Session;
        if (!string.IsNullOrEmpty(controlFlag) && !session.GetFlag(controlFlag))
        {
            Visible = false;
            return;
        }
        else
        {
            Visible = true;
        }

        var player = CollideFirst<Player>();
        // on player enter
        if (!playerIn && player is not null)
        {
            playerIn = true;
            foreach (var entity in Scene.Tracker.GetEntities<PlayerIndicatorZone>())
            {
                var zone = (PlayerIndicatorZone)entity;
                if (zone.zoneMode is ZoneMode.Toggle)
                    zone.lastPlayer = null;
            }
            lastPlayer = player;
            switch (flagMode)
            {
            case FlagMode.Zone:
            case FlagMode.Enable:
                session.SetFlag(flag, true);
                break;
            case FlagMode.Disable:
                session.SetFlag(flag, false);
                break;
            }
        }

        // on player leave
        if (playerIn && player is null)
        {
            playerIn = false;
            if (flagMode is FlagMode.Zone)
                session.SetFlag(flag, false);
        }
    }

    public override void Render()
    {
        base.Render();

        // these are doing 'MathF.Floor'
        float left = (int)Left;
        float right = (int)Right;
        float top = (int)Top;
        float bottom = (int)Bottom;

        float step = renderContinuousLine ? 3f : 2f;

        if (renderInside)
            Draw.Rect(left + 4, top + 4, Width - 8f, Height - 8f, zoneColor * 0.25f);

        if (renderBorder)
        {
            for (float x = left; x < (right - 3); x += 3f)
            {
                // top
                Draw.Line(x, top, x + step, top, zoneColor);
                // bottom
                Draw.Line(x, bottom - 1, x + step, bottom - 1, zoneColor);
            }
            for (float y = top; y < (bottom - 3); y += 3f)
            {
                // left
                Draw.Line(left + 1, y, left + 1, y + step, zoneColor);
                // right
                Draw.Line(right, y, right, y + step, zoneColor);
            }
            // top left
            Draw.Rect(left + 1, top, 1f, 2f, zoneColor);
            // top right
            Draw.Rect(right - 2, top, 2f, 2f, zoneColor);
            // bottom left
            Draw.Rect(left, bottom - 2, 2f, 2f, zoneColor);
            // bottom right
            Draw.Rect(right - 2, bottom - 2, 2f, 2f, zoneColor);
        }

        switch (zoneMode)
        {
        case ZoneMode.Limited:
            if (playerIn)
                DrawIcons(lastPlayer.Position);
            break;
        case ZoneMode.Toggle:
            if (lastPlayer is not null)
                DrawIcons(lastPlayer.Position);
            break;
        case ZoneMode.None:
            // do nothing
            break;
        }
    }

    private void DrawIcons(Vector2 at)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            Vector2 offset = iconOffsets.Count > i ? iconOffsets[i] : Vector2.Zero;
            Color color = iconColors.Count > i ? iconColors[i] : Color.White;
            icons[i].DrawCentered(at + offset, color);
        }
    }
}