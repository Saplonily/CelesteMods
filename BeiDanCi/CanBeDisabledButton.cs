namespace Celeste.Mod.BeiDanCi;

public sealed class CanBeDisabledButton : TextMenu.Button
{
    public CanBeDisabledButton(string label)
        : base(label)
    {
    }

    public override void ConfirmPressed()
    {
        if (!Disabled)
            base.ConfirmPressed();
    }
}
