using Mono.Cecil.Cil;

namespace Celeste.Mod.SpikeDeathListener;

public class SpikeDeathListenerModule : EverestModule
{
    public override void Load()
    {
        IL.Celeste.Spikes.OnCollide += Spikes_OnCollide;
    }

    public override void Unload()
    {
        IL.Celeste.Spikes.OnCollide -= Spikes_OnCollide;
    }

    private void Spikes_OnCollide(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchCallvirt<Player>("Die")))
        {
            cur.Emit(OpCodes.Ldarg_0);
            cur.EmitDelegate((Spikes spikes) =>
            {
                var entities = spikes.Scene.Tracker.GetEntities<SpikeDeathListener>();
                foreach(var entity in entities)
                {
                    SpikeDeathListener c = (SpikeDeathListener)entity;
                    spikes.SceneAs<Level>().Session.SetFlag(c.Flag);
                }
            });
        }
    }
}