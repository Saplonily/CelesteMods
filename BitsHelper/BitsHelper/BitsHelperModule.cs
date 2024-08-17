using Celeste.Mod.BitsHelper.Entities;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperModule : EverestModule
{
    public static BitsHelperModule Instance { get; private set; }

    public override Type SessionType => typeof(BitsHelperSession);
    public static BitsHelperSession Session => (BitsHelperSession)Instance._Session;

    public SpriteBank SpriteBank { get; private set; }
    public MTexture BlowBubbleIndicatorTexture { get; private set; }

    public override void Load()
    {
        Instance = this;
        BubbleCollider.Load();
        FloatingBubble.Load();
        BlowBubble.Load();
        FacingToggleSwapBlock.Load();
    }

    public override void Unload()
    {
        BubbleCollider.Unload();
        FloatingBubble.Unload();
        BlowBubble.Unload();
        FacingToggleSwapBlock.Unload();
    }

    public override void LoadContent(bool firstLoad)
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/BitsHelper/Sprites.xml");
        BlowBubbleIndicatorTexture = GFX.Game["BitsHelper/blowBubbleIndicator"];
    }
}