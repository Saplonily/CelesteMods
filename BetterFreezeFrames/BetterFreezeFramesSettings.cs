namespace Celeste.Mod.BetterFreezeFrames;

public class BetterFreezeFramesSettings : EverestModuleSettings
{
    public bool Enabled { get; set; } = true;

    public bool DebugEnabled { get; set; } = false;

    public void CreateEnabledEntry(TextMenu menu, bool inGame)
    {
        menu.Add(new TextMenu.OnOff("Enabled", Enabled).Change(v =>
        {
            if (v)
            {
                Enabled = true;
                Bff.LoadMain();
            }
            else
            {
                Bff.UnloadMain();
                Enabled = false;
            }
        }));
    }

    public void CreateDebugEnabledEntry(TextMenu menu, bool inGame)
    {
        var onOff = new TextMenu.OnOff("Debug Keybind Enabled", DebugEnabled);
        onOff.Change(v =>
        {
            if (v)
            {
                DebugEnabled = true;
                Bff.LoadDebug();
            }
            else
            {
                Bff.UnloadDebug();
                DebugEnabled = false;
            }
        });
        menu.Add(onOff);
        onOff.AddDescription(menu, Dialog.Clean("modoptions_betterfreezeframes_debugenabled"));
    }
}