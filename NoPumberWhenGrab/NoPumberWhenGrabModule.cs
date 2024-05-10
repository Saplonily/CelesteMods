namespace Celeste.Mod.NoPumberWhenGrab;

public class NoPumberWhenGrabModule : EverestModule
{
    public override void Load()
    {
        On.Celeste.Bumper.OnPlayer += Bumper_OnPlayer;
    }

    private void Bumper_OnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player)
    {
        if (!Input.GrabCheck)
            orig(self, player);
    }

    public override void Unload()
    {
        On.Celeste.Bumper.OnPlayer -= Bumper_OnPlayer;
    }
}