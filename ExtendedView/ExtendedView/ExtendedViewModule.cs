namespace Celeste.Mod.ExtendedView;

public class ExtendedViewModule : EverestModule
{
    public const float Mul = 2f;

    public override void Load()
    {
        VanillaHooks.Load();
    }

    public override void Initialize()
    {
        {
            EverestModuleMetadata meta = new() { Name = "FrostHelper", Version = new Version(1, 0, 0) };
            if (Everest.Loader.TryGetDependency(meta, out var module))
                FrostHelperHooks.Load(module.GetType().Assembly);
        }
        {
            EverestModuleMetadata meta = new() { Name = "VivHelper", Version = new Version(1, 0, 0) };
            if (Everest.Loader.TryGetDependency(meta, out var module))
                VivHelperHooks.Load(module.GetType().Assembly);
        }
    }

    public override void Unload()
    {
        VanillaHooks.Unload();
        FrostHelperHooks.Unload();
    }

    public static void Replace320x180(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcI4(320) || ins.MatchLdcI4(180)))
        {
            cur.EmitConvR4();
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
            cur.EmitConvI4();
            cur.EmitLdcI4(1);
            cur.EmitAdd();
        }
        cur.Index = 0;
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(320f) || ins.MatchLdcR4(180f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
        cur.Index = 0;
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(160f) || ins.MatchLdcR4(90f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
        cur.Index = 0;
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(322f) || ins.MatchLdcR4(182f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }
}