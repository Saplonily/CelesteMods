using YamlDotNet.Serialization;

namespace Celeste.Mod.ChroniaHelperIndicatorZone;

public sealed class ChroniaHelperIndicatorZoneModuleSession : EverestModuleSession
{
    [YamlIgnore] public List<MTexture> Icons { get; set; }
    [YamlIgnore] public List<Vector2> IconOffsets { get; set; }
    [YamlIgnore] public List<Color> IconColors { get; set; }

    public List<string> IconsSave { get; set; }
    public List<Vector2> IconOffsetsSave { get; set; }
    public List<string> IconColorsSave { get; set; }
    public int ZoneDepth { get; set; }

    public void ProcessZoneSaves()
    {
        if (Icons is not null) return;
        Icons = IconsSave?.Select(s => GFX.Game[s]).ToList();
        IconOffsets = IconOffsetsSave;
        IconColors = IconColorsSave?.Select(Calc.HexToColor).ToList();
    }

    public void RecordZoneSave(PlayerIndicatorZone zone)
    {
        if (zone is null)
        {
            Icons = null;
            IconOffsets = null;
            IconColors = null;
            IconsSave = null;
            IconOffsetsSave = null;
            IconColorsSave = null;
            return;
        }
        Icons = zone.Icons;
        IconsSave = zone.Icons.Select(t => t.AtlasPath).ToList();
        IconOffsets = zone.IconOffsets;
        IconOffsetsSave = zone.IconOffsets;
        IconColors = zone.IconColors;
        IconColorsSave = zone.IconColors.Select(c => $"{c.R:X2}{c.G:X2}{c.B:X2}").ToList();
        ZoneDepth = zone.Depth;
    }
}
