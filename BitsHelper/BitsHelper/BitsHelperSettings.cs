using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperSettings : EverestModuleSettings
{
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.S)]
    public ButtonBinding SwitchEgo { get; set; }
}
