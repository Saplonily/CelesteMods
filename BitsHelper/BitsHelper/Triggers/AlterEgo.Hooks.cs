using System.Runtime.CompilerServices;
using Celeste.Mod.BitsHelper.Entities;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BitsHelper;

public static class AlterEgo
{
    private static ILHook playerOrigUpdateILHook;

    public static void Load()
    {
        On.Celeste.Player.Update += Player_Update;
        playerOrigUpdateILHook = new(typeof(Player).GetMethod("orig_Update"), Player_orig_Update);
        IL.Celeste.Level.EnforceBounds += Level_EnforceBounds;
    }

    public static void Unload()
    {
        On.Celeste.Player.Update -= Player_Update;
        playerOrigUpdateILHook.Dispose();
        IL.Celeste.Level.EnforceBounds -= Level_EnforceBounds;
    }

    private static void Player_orig_Update(ILContext il)
    {
        ILCursor cur = new(il);

        cur.GotoNext(ins => ins.MatchLdfld<Player>(nameof(Player.ForceCameraUpdate)));
        cur.GotoPrev(MoveType.After, ins => ins.MatchCallvirt<Player>("get_InControl"));

        cur.EmitLdarg0();
        cur.EmitDelegate(IsCurrent);
        cur.EmitAnd();
    }

    private static void Level_EnforceBounds(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchCallvirt<MapData>(nameof(MapData.CanTransitionTo))))
        {
            cur.EmitLdarg1();
            cur.EmitDelegate(IsOriginalAndCurrent);
            cur.EmitAnd();
        }
    }

    private static bool IsCurrent(Player player)
    {
        var controller = AlterEgoController.Get(player.Scene);
        if (controller is null)
            return true;
        return controller.IsCurrent(player);
    }

    private static bool IsOriginalAndCurrent(Player player)
    {
        var controller = AlterEgoController.Get(player.Scene);
        if (controller is null)
            return true;
        return controller.IsOriginal(player) && controller.IsCurrent(player);
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        var controller = AlterEgoController.Get(self.Scene);
        if (controller is null)
        {
            orig(self);
            return;
        }
        else
        {
            if (controller.IsCurrent(self))
            {
                orig(self);
                if (!Input.GrabCheck)
                    foreach (AlterEgoBoopable comp in self.Scene.Tracker.GetComponents<AlterEgoBoopable>())
                        comp.CheckAndAct(self);
            }
            else
            {
                // uh totally a mess
                // we save all inputs, set to 0, and restore them
                // luckily at least there's an MInput.Disabled to force all button inputs to return false

                var axisInputsCount = controller.VirtualAxisInputs.Length;
                Span<int> previousValues = stackalloc int[axisInputsCount];
                Span<int> previousPreviousValues = stackalloc int[axisInputsCount];
                int index = 0;
                foreach (var i in controller.VirtualAxisInputs)
                {
                    if (i is VirtualAxis va)
                    {
                        previousValues[index] = Unsafe.BitCast<float, int>(va.Value);
                        previousPreviousValues[index] = Unsafe.BitCast<float, int>(va.PreviousValue);
                        SetVirtualAxisValue(va, 0f);
                        SetVirtualAxisPreviousValue(va, 0f);
                        index++;
                    }
                    else if (i is VirtualIntegerAxis via)
                    {
                        previousValues[index] = via.Value;
                        previousPreviousValues[index] = via.PreviousValue;
                        via.Value = 0;
                        via.PreviousValue = 0;
                        index++;
                    }
                }

                var pGrabMode = Settings.Instance.GrabMode;
                bool pDisabled = MInput.Disabled;
                MInput.Disabled = true;
                Settings.Instance.GrabMode = GrabModes.Hold;
                try
                {
                    orig(self);
                }
                finally
                {
                    MInput.Disabled = pDisabled;
                    Settings.Instance.GrabMode = pGrabMode;

                    index = 0;
                    foreach (var i in controller.VirtualAxisInputs)
                    {
                        if (i is VirtualAxis va)
                        {
                            SetVirtualAxisValue(va, Unsafe.BitCast<int, float>(previousValues[index]));
                            SetVirtualAxisPreviousValue(va, Unsafe.BitCast<int, float>(previousPreviousValues[index]));
                            index++;
                        }

                        else if (i is VirtualIntegerAxis via)
                        {
                            via.Value = previousValues[index];
                            via.PreviousValue = previousPreviousValues[index];
                            index++;
                        }
                    }
                }

                // why publicizer didn't publicize Monocle

                [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Value")]
                static extern void SetVirtualAxisValue(VirtualAxis va, float value);

                [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_PreviousValue")]
                static extern void SetVirtualAxisPreviousValue(VirtualAxis va, float value);
            }
        }
    }

}
