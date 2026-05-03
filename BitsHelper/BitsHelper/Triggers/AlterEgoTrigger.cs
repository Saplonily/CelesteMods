using Celeste.Mod.BitsHelper.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BitsHelper.Triggers;

public enum AlterEgoVisualMode
{
    SameAsPlayer = 0,
    Backpack = 1,
    NoBackpack = 2,
    MadelineAsBadeline = 3
}

[CustomEntity("BitsHelper/AlterEgoTrigger")]
public sealed class AlterEgoTrigger : Trigger
{
    private readonly AlterEgoVisualMode visualMode;
    private readonly Vector2 alterEgoSpawnOffset;

    public AlterEgoTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        alterEgoSpawnOffset = data.Nodes[0] + offset - Position;
        visualMode = (AlterEgoVisualMode)data.Int("visualMode", (int)AlterEgoVisualMode.MadelineAsBadeline);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        var controller = AlterEgoController.GetOrCreate(Scene, player);
        Player newPlayer = new Player(Position + alterEgoSpawnOffset, visualMode switch
        {
            AlterEgoVisualMode.SameAsPlayer => player.Sprite.Mode,
            AlterEgoVisualMode.Backpack => PlayerSpriteMode.Madeline,
            AlterEgoVisualMode.NoBackpack => PlayerSpriteMode.MadelineNoBackpack,
            AlterEgoVisualMode.MadelineAsBadeline => PlayerSpriteMode.MadelineAsBadeline,
            _ => PlayerSpriteMode.MadelineAsBadeline
        });
        Scene.Add(newPlayer);
        controller.RegisterNew(newPlayer);
        RemoveSelf();
    }
}
