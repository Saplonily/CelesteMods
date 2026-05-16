using Celeste.Mod.BitsHelper.Entities;
using MonoMod.ModInterop;

namespace Celeste.Mod.BitsHelper;

[ModExportName("BitsHelper.AlterEgo")]
public static class BitsHelperExports
{
    public static bool TrySwitchToNext(Scene scene)
    {
        var c = AlterEgoController.Get(scene);
        if (c is null)
            return false;
        return c.TrySwitchToNext();
    }

    // more?
}
