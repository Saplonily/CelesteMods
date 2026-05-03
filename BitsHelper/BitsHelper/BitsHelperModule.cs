using Celeste.Mod.BitsHelper.Entities;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperModule : EverestModule
{
    public static BitsHelperModule Instance { get; private set; }

    public SpriteBank SpriteBank { get; private set; }

    public override Type SettingsType => typeof(BitsHelperSettings);

    public static BitsHelperSettings Settings => (BitsHelperSettings)Instance._Settings;

    public override void Load()
    {
        Instance = this;
        BubbleCollider.Load();
        AlterEgo.Load();
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