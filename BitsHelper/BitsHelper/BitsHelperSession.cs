using YamlDotNet.Serialization;

namespace Celeste.Mod.BitsHelper;

public sealed class BitsHelperSession : EverestModuleSession
{
    public int BlowBubbleCount { get; set; }

    [YamlIgnore] public AlterEgo.AlterEgoState AlterEgo { get; set; }
}