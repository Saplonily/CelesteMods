using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BitsHelper.Entities;

public abstract class FloatingBubbleEmitter : Entity
{
    protected readonly Sprite sprite;
    protected bool firing;

    public FloatingBubbleEmitter(Vector2 position, bool attach) : base(position)
    {
        Add(sprite = BitsHelperModule.Instance.SpriteBank.Create("bubbleEmitter"));
        sprite.CenterOrigin();

        if (attach)
        {
            Add(new StaticMover()
            {
                OnShake = shake => sprite.Position += shake,
                OnMove = move => Position += move,
                SolidChecker = solid => solid.CollidePoint(Position + new Vector2(0f, 16f)),
                JumpThruChecker = jumpthru => jumpthru.CollidePoint(Position + new Vector2(0f, 16f))
            });
        }
    }

    public void Fire()
    {
        if (firing) return;
        Add(new Coroutine(SpawnRoutine()));
    }

    private IEnumerator SpawnRoutine()
    {
        firing = true;
        sprite.Play("open");
        while (sprite.CurrentAnimationFrame != 1)
            yield return null;
        Scene.Add(new FloatingBubble(new Vector2(Position.X, Position.Y - 18), Vector2.Zero));
        Audio.Play("event:/BitsHelper/bubblefx/bubble_appear", Position);
        yield return null;
        firing = false;
    }
}
