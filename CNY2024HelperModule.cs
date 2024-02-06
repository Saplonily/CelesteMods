
namespace Celeste.Mod.CNY2024Helper;

public class CNY2024HelperModule : EverestModule
{
    public static CNY2024HelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(CNY2024HelperSettings);
    public static CNY2024HelperSettings Settings => (CNY2024HelperSettings)Instance._Settings;

    public override void Load()
    {
        Instance = this;
        DrivableCarLightModule.Load();
        VivCassetteTileEntityFix.Load();
    }

    public override void Unload()
    {
        DrivableCarLightModule.Unload();
        VivCassetteTileEntityFix.Unload();
    }
}