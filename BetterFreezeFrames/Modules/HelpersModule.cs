using VortexPufferBarrierRenderer = Celeste.Mod.VortexHelper.Entities.PufferBarrierRenderer;
using VortexPufferBarrier = Celeste.Mod.VortexHelper.Entities.PufferBarrier;
using VivHelperEntities = global::VivHelper.Entities;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BetterFreezeFrames;

public static class HelpersModule
{
    private static bool femtoHelperLoaded;
    private static bool isaGrabBagLoaded;
    private static bool vivHelperLoaded;
    private static bool flaglinesAndSuchLoaded;
    private static bool vortexHelperLoaded;
    private static bool pandorasBoxLoaded;

    private static ILHook VortexHelperBarrierILHook;

    public static void CheckLoadedStates()
    {
        femtoHelperLoaded = Everest.Loader.DependencyLoaded(new() { Name = "FemtoHelper", Version = new(1, 11, 5) });
        isaGrabBagLoaded = Everest.Loader.DependencyLoaded(new() { Name = "IsaGrabBag", Version = new(1, 6, 14) });
        vivHelperLoaded = Everest.Loader.DependencyLoaded(new() { Name = "VivHelper", Version = new(1, 14, 5) });
        flaglinesAndSuchLoaded = Everest.Loader.DependencyLoaded(new() { Name = "FlaglinesAndSuch", Version = new(1, 6, 19) });
        vortexHelperLoaded = Everest.Loader.DependencyLoaded(new() { Name = "VortexHelper", Version = new(1, 2, 14) });
        pandorasBoxLoaded = Everest.Loader.DependencyLoaded(new() { Name = "PandorasBox", Version = new(1, 0, 49) });
    }

    public static bool IsSafeToUpdate(Entity entity)
    {
        return (femtoHelperLoaded && CheckFemtoHelper(entity)) ||
               (isaGrabBagLoaded && CheckIsaGrabBag(entity)) ||
               (vivHelperLoaded && CheckVivHelper(entity)) ||
               (flaglinesAndSuchLoaded && CheckFlaglinesAndSuch(entity)) ||
               (vortexHelperLoaded && CheckVortexHelper(entity)) ||
               (pandorasBoxLoaded && CheckPandorasBox(entity));

        static bool CheckVivHelper(Entity entity)
            => entity is VivHelperEntities.CustomTorch or VivHelperEntities.CustomTorch2 or VivHelperEntities.AnimatedSpinner;

        static bool CheckFemtoHelper(Entity entity)
            => entity is FemtoHelper.PseudoPolyhedron or CustomMoonCreature; // wtf it's in global namespace

        static bool CheckIsaGrabBag(Entity entity)
            => entity is IsaGrabBag.DreamSpinnerRenderer;

        static bool CheckFlaglinesAndSuch(Entity entity)
            => entity is global::FlaglinesAndSuch.CustomFlagline;

        static bool CheckVortexHelper(Entity entity)
            => entity is VortexPufferBarrierRenderer or VortexPufferBarrier;

        static bool CheckPandorasBox(Entity entity)
            => entity is PandorasBox.ColoredWater;
    }

    public static void Load()
    {
        if (vortexHelperLoaded)
            LoadVortexHelper();
    }

    public static void Unload()
    {
        if (vortexHelperLoaded)
            UnloadVortexHelper();
    }

    private static void LoadVortexHelper() 
        => VortexHelperBarrierILHook = new ILHook(typeof(VortexPufferBarrierRenderer).GetMethod("Update"), HookUtils.ILHookReplaceOnInterval);

    private static void UnloadVortexHelper() 
        => VortexHelperBarrierILHook?.Dispose();
}
