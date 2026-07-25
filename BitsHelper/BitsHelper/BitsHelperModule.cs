using Celeste.Mod.BitsHelper.Entities;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperModule : EverestModule
{
    public static BitsHelperModule Instance { get; private set; }

    public SpriteBank SpriteBank { get; private set; }

    public override Type SettingsType => typeof(BitsHelperSettings);

    public static BitsHelperSettings Settings => (BitsHelperSettings)Instance._Settings;

    private static readonly DetourConfig DetourConfig = new("BitsHelper", 32);

    public override void Load()
    {
        Instance = this;
        BubbleCollider.Load();
        // low priority to "capture" more inputs
        using (new DetourConfigContext(DetourConfig).Use())
        {
            AlterEgo.Load();
        }
    }

    public override void Unload()
    {
        BubbleCollider.Unload();
        AlterEgo.Unload();
    }

    public override void LoadContent(bool firstLoad)
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/BitsHelper/Sprites.xml");
    }
}