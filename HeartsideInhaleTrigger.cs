using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.CNY2024Helper.Triggers;

[CustomEntity("CNY2024Helper/HeartsideInhaleTrigger")]
public class HeartsideInhaleTrigger : Trigger
{
    private Vector2 target;
    private float minimumDis;
    private float strength;
    private string flag;
    private float radius;
    private float k;

    public HeartsideInhaleTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        strength = data.Float("strength", 400f);
        minimumDis = data.Float("minimumdis", 10f);
        k = data.Float("k", 10f);
        target = data.NodesOffset(offset)[0];
        flag = data.Attr("flag");
    }

    public Vector2 CalcTangentLine(Vector2 O, Vector2 Node, float R)
    {
        Vector2 e, f, g, h;
        e = new Vector2();
        f = new Vector2();
        g = new Vector2();
        h = new Vector2();
        e.X = Node.X - O.X;
        e.Y = Node.Y - O.Y;
        float t = (float)(R / Math.Sqrt(e.X * e.X + e.Y * e.Y));
        f.X = e.X * t;
        f.Y = e.Y * t;
        float a = (float)Math.Acos(t);
        if ((e.X <= 0 && e.Y >= 0) || (e.X <= 0 && e.Y <= 0)) a = -a;
        g.X = (float)(f.X * Math.Cos(a) - f.Y * Math.Sin(a));
        g.Y = (float)(f.X * Math.Sin(a) + f.Y * Math.Cos(a));
        h.X = g.X + O.X;
        h.Y = g.Y + O.Y;
        return h;
    }

    public IEnumerator Inhale(Player player)
    {
        Vector2 Tengency = CalcTangentLine(target, player.Position, (float)(radius * (0.4)));
        player.Speed = Vector2.Normalize(target - player.Position) * k;
        int colddown = 30;
        float angleK = -16;
        float angle;
        while (radius > minimumDis)
        {
            if (colddown == 0)
            {
                angle = (float)(Math.PI / 180) * angleK;
                radius = Vector2.Distance(target, player.Position);
                Vector2 NextPos = target;
                float t1 = (float)((player.X - target.X) * Math.Cos(angle));
                float t2 = (float)((player.Y - target.Y) * Math.Sin(angle));
                NextPos.X += (float)(t1 - t2);
                NextPos.Y += (float)((player.Y - target.Y) * Math.Cos(angle) + (player.X - target.X) * Math.Sin(angle));
                Vector2 Speed = Vector2.Normalize(NextPos - player.Position);
                float XxX = Vector2.Dot(player.Speed, Speed);
                if (XxX < 0) 
                {
                    angleK = -angleK;
                    continue;
                }
                player.Speed = Speed * k;
                //angleK += 0.1f;
            }
            else
            {
                if (k < strength) k += 40f;
                player.Speed = Vector2.Normalize(Tengency - player.Position) * k;
                if (Vector2.Distance(player.Position, Tengency) < 3f)
                {
                    colddown = 0;
                    continue;
                }
            }
            yield return null;
        }
        var level = SceneAs<Level>();
        level.Session.SetFlag(flag, true);
        yield break;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        player.StateMachine.State = Player.StDummy;
        player.DummyFriction = false;
        player.DummyGravity = false;
        player.ForceCameraUpdate = true;
        radius = Vector2.Distance(player.Position, target);
        var coroutine = new Coroutine(Inhale(player));
        Add(coroutine);
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Circle(target, 4f, Color.Red, 1);
    }
}
