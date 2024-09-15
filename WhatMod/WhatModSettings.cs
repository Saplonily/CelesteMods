using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.WhatMod;

public sealed class WhatModSettings : EverestModuleSettings
{
    [DefaultButtonBinding(Buttons.Y,Keys.W)]
    public ButtonBinding What { get; set; }

    public bool PauseWhenWhat { get; set; }
}
