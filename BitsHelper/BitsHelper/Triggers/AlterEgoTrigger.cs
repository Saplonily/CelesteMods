using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

[CustomEntity("BitsHelper/AlterEgoTrigger")]
public sealed class AlterEgoTrigger : Trigger
{
    private Vector2 spawnPosition;

    public AlterEgoTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        spawnPosition = data.Nodes[0] + offset;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        BitsHelperModule.Session.AlterEgo = null;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (Scene.Tracker.CountEntities<Player>() > 1)
            return;
        AlterEgo.SpawnAlter(player, spawnPosition);
    }
}