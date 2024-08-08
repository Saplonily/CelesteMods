using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using VivHelper;
using VivHelper.Entities;

namespace Celeste.Mod.CNY2024Helper;

// blame viv

public static class VivCassetteTileEntityFix
{
    private static ILHook vivOnLoadEntityILHook;

    public static void Load()
    {
        if (vivOnLoadEntityILHook is not null) return;
        if (CNY2024HelperModule.Instance is not null)
            if (!CNY2024HelperModule.Settings.VivCassetteTileEntityFix)
                return;
        try
        {
            Type type = typeof(VivHelperModule);
            MethodInfo method = type.GetMethod("Level_OnLoadEntity", BindingFlags.NonPublic | BindingFlags.Instance);
            vivOnLoadEntityILHook = new(method, VivOnLoadEntityILHook);
        }
        catch (Exception ex)
        {
            Logger.LogDetailed(ex, $"CNY2024Helper/VivCassetteTileEntityFix");
        }
    }

    private static void VivOnLoadEntityILHook(ILContext ilc)
    {
        ILCursor cur = new(ilc);
        if (!cur.TryGotoNext(ins => ins.MatchNewobj<CassetteTileEntity>()))
        {
            // if currently vivhelper doesn't fix this
            Logger.Log(LogLevel.Info, "CNY2024Helper", "Fixing VivCassetteTileEntity...");
            cur.Index = 0;
            if (cur.TryGotoNext(ins => ins.MatchLdsfld<VivHelperModule>("createdCassetteManager")))
            {
                cur.Emit(OpCodes.Ldarg, 1); // load 'Level'
                cur.Emit(OpCodes.Ldarg, 4); // load 'LevelData'
                cur.Emit(OpCodes.Ldarg, 3); // load 'offset'
                // new a CassetteTileEntity
                cur.Emit(OpCodes.Newobj, typeof(CassetteTileEntity).GetConstructor([typeof(EntityData), typeof(Vector2)]));
                // add to level
                cur.Emit(OpCodes.Call, typeof(Scene).GetMethod("Add", [typeof(Entity)]));
            }
        }
    }

    public static void Unload()
    {
        vivOnLoadEntityILHook?.Dispose();
        vivOnLoadEntityILHook = null;
    }
}