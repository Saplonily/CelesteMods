namespace Celeste.Mod.BeiDanCi;

public sealed class DisabledNoSoundButton : TextMenu.Button
{
    public DisabledNoSoundButton(string label)
        : base(label)
    {
    }

    public override void ConfirmPressed()
    {
        if (!Disabled)
            base.ConfirmPressed();
    }
}
