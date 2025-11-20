namespace Celeste.Mod.RelativeMadeline;

public sealed class RelativeMadelineSettings : EverestModuleSettings
{
    [SettingRange(1, 60, true)]
    public int SpeedThreshold { get; set; } = 18;

    [SettingRange(1, 60, true)]
    public int SpeedTarget { get; set; } = 18;
}