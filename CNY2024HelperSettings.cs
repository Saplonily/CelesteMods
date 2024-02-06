namespace Celeste.Mod.CNY2024Helper;

public class CNY2024HelperSettings : EverestModuleSettings
{
    private bool vivCassetteTileEntityFix = true;

    public bool VivCassetteTileEntityFix
    {
        get => vivCassetteTileEntityFix;
        set
        {
            if (value)
            {
                CNY2024Helper.VivCassetteTileEntityFix.Load();
            }
            else
            {
                CNY2024Helper.VivCassetteTileEntityFix.Unload();
            }
            vivCassetteTileEntityFix = value;
        }
    }
}