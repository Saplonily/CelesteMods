using System.Reflection;
using Celeste.Mod.MaxHelpingHand.Entities;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BitsHelper;

partial class BubbleCollider
{
    private static Hook FlagTouchSwitch_setUpCollision;

    public static void Load()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 += OnTouchSwitchCtor;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += OnSpringCtor;
        // it's protected virtual, so i think it could be somehow "stable"
        FlagTouchSwitch_setUpCollision = new Hook(
            typeof(FlagTouchSwitch).GetMethod("setUpCollision", BindingFlags.NonPublic | BindingFlags.Instance),
            OnFlagTouchSwitchCtor
        );
    }

    public static void Unload()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 -= OnTouchSwitchCtor;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= OnSpringCtor;
        FlagTouchSwitch_setUpCollision.Dispose();
    }

    private static void OnTouchSwitchCtor(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position)
    {
        orig(self, position);
        self.Add(new BubbleCollider(f => self.TurnOn()));
    }

    private static void OnSpringCtor(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        self.Add(new BubbleCollider(f => f.OnSpring(self)));
    }

    private static void OnFlagTouchSwitchCtor(Action<FlagTouchSwitch, EntityData> orig, FlagTouchSwitch self, EntityData data)
    {
        orig(self, data);
        self.Add(new BubbleCollider(f => self.TurnOn()));
    }
}
