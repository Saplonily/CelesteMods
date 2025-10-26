namespace Celeste.Mod.BeiDanCi;

public sealed class MultilineButton : TextMenu.Button
{
    public MultilineButton(string label)
        : base(label)
    {
    }

    public override float Height()
        => (Label.Count(c => c is '\n') + 1) * ActiveFont.LineHeight;
}
