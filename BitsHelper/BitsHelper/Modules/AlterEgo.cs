using System;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.BitsHelper;

public static class AlterEgo
{
    private static Hook HookPlayer_get_InControl;
    public static int AlterEgoStateIndex;

    public sealed class AlterEgoState
    {
        public Player Self;
        public Player Alter;
    }

    public static void Load()
    {
        On.Celeste.Level.Update += Level_Update;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.Die += Player_Die;
        // TODO optimize with ILHook
        HookPlayer_get_InControl = new Hook(typeof(Player).GetProperty("InControl").GetGetMethod(), Player_get_InControl);
    }

    public static void Unload()
    {
        On.Celeste.Level.Update -= Level_Update;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.Die -= Player_Die;
        HookPlayer_get_InControl.Dispose();
    }

    public static Player SpawnAlter(Player player, Vector2 spawnPosition)
    {
        var state = BitsHelperModule.Session.AlterEgo = new AlterEgoState();
        player.Add(CreatePlayerHoldable(player));
        Player alter = new(spawnPosition, PlayerSpriteMode.MadelineAsBadeline);
        state.Self = player;
        state.Alter = alter;
        alter.Add(CreatePlayerHoldable(alter));
        player.Scene.Add(alter);
        return alter;
    }

    public static Holdable CreatePlayerHoldable(Player player)
    {
        var holdable = new Holdable();
        holdable.SpeedGetter = () => player.Speed;
        holdable.SpeedSetter = s => player.Speed = s;
        holdable.OnRelease = f => { player.Speed = f * 200f; player.StateMachine.State = Player.StNormal; };
        holdable.OnPickup = () => { player.StateMachine.State = Player.StFrozen; };
        return holdable;
    }

    private delegate bool orig_get_InControl(Player self);
    private static bool Player_get_InControl(orig_get_InControl orig, Player self)
    {
        bool r = orig(self);
        return r && (BitsHelperModule.Session.AlterEgo?.Alter != self);
    }

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        var state = BitsHelperModule.Session.AlterEgo;
        if (state is null || self == state.Self)
        {
            orig(self);
            return;
        }
        if (self == state.Alter)
        {
            // HELP hard-coded compatibility issue 
            var p = MInput.Disabled;
            var pmx = Input.MoveX.Value;
            var pmy = Input.MoveY.Value;
            var pgm = Input.GliderMoveY.Value;
            var pa = Input.Aim.Value;
            var pf = Input.Feather.Value;

            MInput.Disabled = true;
            Input.MoveX.Value = 0;
            Input.MoveY.Value = 0;
            Input.GliderMoveY.Value = 0;
            Input.Aim.Value = Vector2.Zero;
            Input.Feather.Value = Vector2.Zero;

            orig(self);

            Input.MoveX.Value = pmx;
            Input.MoveY.Value = pmy;
            Input.GliderMoveY.Value = pgm;
            Input.Aim.Value = pa;
            Input.Feather.Value = pf;
            MInput.Disabled = p;
        }
    }

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        var state = BitsHelperModule.Session.AlterEgo;
        if (state is null)
            return;
        var key = BitsHelperModule.Settings.SwitchEgo;
        if (key.Pressed)
        {
            key.ConsumePress();
            state.Self.ResetSprite(state.Self.DefaultSpriteMode);
            (state.Self, state.Alter) = (state.Alter, state.Self);
            var list = self.Tracker.Entities[typeof(Player)];
            var selfIndex = list.IndexOf(state.Self);
            var alterIndex = list.IndexOf(state.Alter);
            (list[selfIndex], list[alterIndex]) = (list[alterIndex], list[selfIndex]);
        }
    }

    private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig,
        Player self,
        Vector2 direction,
        bool evenIfInvincible,
        bool registerDeathInStats
        )
    {
        var ret = orig(self, direction, evenIfInvincible, registerDeathInStats);
        var state = BitsHelperModule.Session.AlterEgo;
        if (state is null)
            return ret;

        if (state.Self.Dead && !state.Alter.Dead)
            state.Alter.Die(direction, evenIfInvincible, false);
        else if (state.Alter.Dead && !state.Self.Dead)
            state.Self.Die(direction, evenIfInvincible, false);

        return ret;
    }
}
