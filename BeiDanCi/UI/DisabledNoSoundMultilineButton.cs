namespace Celeste.Mod.BeiDanCi;

public sealed class DisabledNoSoundMultilineButton : TextMenu.Button
{
    public DisabledNoSoundMultilineButton(string label)
        : base(label)
    {
    }

    public override void ConfirmPressed()
    {
        if (!Disabled)
            base.ConfirmPressed();
    }

    public override float Height()
        => (Label.Count(c => c is '\n') + 1) * ActiveFont.LineHeight;
}
