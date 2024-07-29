using Celeste.Mod.MaxHelpingHand.Entities;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste.Mod.BitsHelper.Entities;

partial class FloatingBubble
{
    private static Hook FlagTouchSwitchCtorHook;
    private readonly static ConstructorInfo FlagTouchSwitchCtorInfo =
        typeof(FlagTouchSwitch).GetConstructor([typeof(EntityData), typeof(Vector2)]);

    public static void Load()
    {
        FlagTouchSwitchCtorHook = new Hook(FlagTouchSwitchCtorInfo, OnFlagTouchSwitchCtor);
    }

    public static void Unload()
    {
        FlagTouchSwitchCtorHook.Dispose();
    }

    private static void OnFlagTouchSwitchCtor(Action<FlagTouchSwitch, EntityData, Vector2> orig, FlagTouchSwitch self, EntityData data, Vector2 offset)
    {
        orig(self, data, offset);
        self.Add(new BubbleCollider());
    }
}
