using Celeste.Mod.Entities;

namespace Celeste.Mod.RelativeMadeline;

public sealed class RelativeMadelineModule : EverestModule
{
    public static RelativeMadelineModule Instance { get; private set; }

    public override Type SettingsType => typeof(RelativeMadelineSettings);
    public static RelativeMadelineSettings Settings => (RelativeMadelineSettings)Instance._Settings;

    public override void Load()
    {
        Instance = this;
        On.Celeste.Player.ctor += Player_ctor;
    }

    public override void Unload()
    {
        On.Celeste.Player.ctor -= Player_ctor;
    }

    private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
    {
        orig(self, position, spriteMode);
        self.Add(new RelativeControlComponent());
    }
}