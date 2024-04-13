using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using SDL2;

namespace Celeste.Mod.SafeAltF4;

public sealed class SafeAltF4Module : EverestModule
{
    private static bool failedHookWarned;
    private static bool failedHook;
    private ILHook pollEventsHook;

    public override void Load()
    {
        failedHook = false;
        try
        {
            var assembly = typeof(Game).Assembly;
            string fnaVersion = assembly.GetName().Version.ToString();
            if (fnaVersion != "23.3.0.0")
                throw new Exception($"FNA assembly version not match. 23.3.0.0 expected, got {fnaVersion}.");

            var fnaPlatform = assembly.GetType("Microsoft.Xna.Framework.SDL2_FNAPlatform", true);
            var pollEvents = fnaPlatform.GetMethod("PollEvents", BindingFlags.Static | BindingFlags.Public);
            pollEventsHook = new(pollEvents, PollEventsHook);
        }
        catch (Exception ex)
        {
            failedHook = true;
            On.Celeste.Overworld.Begin += Overworld_Begin;
            Logger.LogDetailed(ex, nameof(SafeAltF4));
            pollEventsHook?.Dispose();
        }
    }

    private void Overworld_Begin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
    {
        if (failedHook && !failedHookWarned)
            Engine.Scene = new PreviewPostcard(new Postcard(Dialog.Get("POSTCARD_SAFEALTF4_FAILEDHOOK", null), 1));
        failedHookWarned = true;
        orig(self);
    }

    public override void Unload()
    {
        pollEventsHook?.Dispose();
        if (failedHook)
            On.Celeste.Overworld.Begin -= Overworld_Begin;
    }

    public static void PollEventsHook(ILContext ilc)
    {
        ILCursor cur = new(ilc);
        bool result = cur.TryGotoNext(
            MoveType.After,
            ins => ins.OpCode == OpCodes.Ldloc_0,
            ins => ins.OpCode == OpCodes.Ldfld,
            ins => ins.OpCode == OpCodes.Ldfld,
            ins => ins.MatchLdcI4(12)
            );
        if (!result) throw new Exception("TryGotoNext returns false.");
        cur.Index--;
        cur.EmitDup();
        cur.EmitDelegate((SDL.SDL_WindowEventID id) =>
        {
            if (id is SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
            {
                if (Engine.Scene is Level)
                {
                    EmergencySave();
                }
            }
        });
    }

    public static void EmergencySave()
    {
        Logger.Log(LogLevel.Warn, nameof(SafeAltF4), "Ooops, someone is closing the window without saving the game, let's help them.");
        UserIO.QueuedSaves ??= new Queue<Tuple<bool, bool>>();

        var co = UserIO.SaveRoutine(true, true);
        while (co.MoveNext())
        {
            if ((Engine.Scene as IOverlayHandler)!.Overlay is FileErrorOverlay overlay)
            {
                Logger.Log(LogLevel.Error, nameof(SafeAltF4), "Ooops! Save failed. Let's give up this emergency save...");
                return;
            }
        }
        Logger.Log(LogLevel.Info, nameof(SafeAltF4), "Finished emergency saving.");
    }
}