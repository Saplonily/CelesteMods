namespace Celeste.Mod.BitsHelper.Entities;

partial class BubbleCollider
{
    public static void Load()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 += OnTouchSwitchCtor;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += OnSpringCtor;
    }

    public static void Unload()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 -= OnTouchSwitchCtor;
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= OnSpringCtor;
    }

    private static void OnTouchSwitchCtor(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position)
    {
        orig(self, position);
        self.Add(new BubbleCollider());
    }

    private static void OnSpringCtor(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        self.Add(new BubbleCollider());
    }
}
