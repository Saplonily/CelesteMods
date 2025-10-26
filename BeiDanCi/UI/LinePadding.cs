namespace Celeste.Mod.BeiDanCi;

internal sealed class LinePadding : TextMenu.Item
{
    public override float Height()
        => ActiveFont.LineHeight;
}
