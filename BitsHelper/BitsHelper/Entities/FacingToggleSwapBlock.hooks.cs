using MonoMod.Utils;

namespace Celeste.Mod.BitsHelper;

partial class FacingToggleSwapBlock
{
    public static void Load()
    {
        On.Celeste.Player.Update += Player_Update;
    }

    public static void Unload()
    {
        On.Celeste.Player.Update -= Player_Update;
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.Scene.Tracker.CountEntities<FacingToggleSwapBlock>() is 0) 
            return;

        Facings cur = self.Facing;
        if (self.StateMachine.State is Player.StClimb || self.StateMachine.PreviousState is Player.StClimb)
            cur = Math.Sign(Input.MoveX.Value) switch
            {
                -1 => Facings.Left,
                1 => Facings.Right,
                _ => cur
            };

        var dd = DynamicData.For(self);
        if (dd.Get("fs_facing") is Facings pre && pre != cur)
        {
            var entities = self.Scene.Tracker.GetEntities<FacingToggleSwapBlock>();
            foreach (var entity in entities)
            {
                FacingToggleSwapBlock block = (FacingToggleSwapBlock)entity;
                block.UpdateWithFacing(cur);
            }
        }
        dd.Set("fs_facing", cur);
    }
}