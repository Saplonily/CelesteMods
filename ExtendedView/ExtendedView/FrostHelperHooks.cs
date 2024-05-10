using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using static Celeste.Mod.ExtendedView.ExtendedViewModule;

namespace Celeste.Mod.ExtendedView;

public static class FrostHelperHooks
{
    private static ILHook spinnerILHook;

    public static void Load(Assembly asm)
    {
        var type = asm.GetType("FrostHelper.CustomSpinner");
        var method = type.GetMethod("InView", BindingFlags.Instance | BindingFlags.NonPublic);
        spinnerILHook = new(method, FrostHelperCustomSpinner_InView);
    }

    public static void Unload()
    {
        spinnerILHook?.Dispose();
    }

    private static void FrostHelperCustomSpinner_InView(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.OpCode == OpCodes.Ldc_R4))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }
}
