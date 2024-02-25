namespace Celeste.Mod.BetterFreezeFrames;

public class BetterFreezeFramesSettings : EverestModuleSettings
{
    private bool enabled;

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;
            if (enabled && !BetterFreezeFramesModule.LoadedStuffs)
                BetterFreezeFramesModule.Instance.Load();
            if (!enabled && BetterFreezeFramesModule.LoadedStuffs)
                BetterFreezeFramesModule.Instance.Unload();
        }
    }
}