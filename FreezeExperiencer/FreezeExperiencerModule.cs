
using FMOD.Studio;

namespace Celeste.Mod.FreezeExperiencer;

public sealed class FreezeExperiencerModule : EverestModule
{
    public override void Load()
    {
    }

    public override void Unload()
    {
    }

    public static void Freeze(Level level)
    {
        while (true)
        {
            var p = level.TimeActive;
            level.TimeActive += Engine.DeltaTime;
            if (p == level.TimeActive)
                break;
        }
        long ticks = TimeSpan.FromSeconds((double)Engine.RawDeltaTime).Ticks;
        while (true)
        {
            var p = level.RawTimeActive;
            level.RawTimeActive += Engine.DeltaTime;
            level.Session.Time += ticks;
            if (p == level.RawTimeActive)
                break;
        }
    }

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
    {
        if (!inGame) return;
        menu.Add(new TextMenu.SubHeader("Freeze Experiencer" + " | v." + Metadata.VersionString));
        TextMenu.Button item = new TextMenu.Button("Freeze");
        item.Pressed(() => { if (Engine.Scene is Level level) Freeze(level); });
        menu.Add(item);
    }
}