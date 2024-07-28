
namespace Celeste.Mod.ChroniaHelperIndicatorZone;

public class ChroniaHelperIndicatorZoneModule : EverestModule
{
    public override Type SessionType => typeof(ChroniaHelperIndicatorZoneModuleSession);
    public static ChroniaHelperIndicatorZoneModule Instance { get; private set; }
    public ChroniaHelperIndicatorZoneModuleSession Session => (ChroniaHelperIndicatorZoneModuleSession)_Session;

    public override void Load()
    {
        Instance = this;
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
    }

    private static void LevelLoader_OnLoadingThread(Level level)
    {
        level.Add(new PlayerIndicatorZone.IconRenderer());
    }

    public override void Unload()
    {
        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
    }
}