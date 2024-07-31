
namespace Celeste.Mod.CNY2024Helper;

public class CNY2024HelperModule : EverestModule
{
    public override void Load()
    {
        DrivableCarLightModule.Load();
    }

    public override void Unload()
    {
        DrivableCarLightModule.Unload();
    }
}