using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.BetterFreezeFrames;

public static class HookUtils
{
    public static void ILHookReplaceOnInterval(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(ins => ins.MatchCallvirt<Scene>("OnInterval")))
        {
            var ins = cur.Instrs[cur.Index];
            ins.OpCode = OpCodes.Call;
            ins.Operand = typeof(Bff).GetMethod(nameof(Bff.OnExtraInterval));
        }
    }

    public static void ILHookSkipBaseUpdate<T>(ILContext il)
    {
        ILCursor cur = new(il);
        cur.GotoNext(MoveType.Before, ins => ins.MatchLdarg0(), ins => ins.MatchCall<T>("Update"));
        Instruction ins = cur.Instrs[cur.Index + 2];
        cur.EmitLdsfld(Bff.FreezeUpdatingField);
        cur.EmitBrtrue(ins);
    }
}