using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperSettings : EverestModuleSettings
{
    [DefaultButtonBinding(0, Keys.Tab)]
    public ButtonBinding SwitchBetweenPlayers { get; set; }
}
