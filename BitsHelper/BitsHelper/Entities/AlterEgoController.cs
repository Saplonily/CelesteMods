using System.Collections.Immutable;

namespace Celeste.Mod.BitsHelper.Entities;

[Tracked]
public sealed class AlterEgoController : Entity
{
    private readonly Player original;
    private readonly List<Player> alterEgos;
    private (int index, Player player) current;
    private bool holdInteractions, boopInteractions;

    public ImmutableArray<VirtualInput> VirtualAxisInputs;

    public AlterEgoController(Player first)
    {
        original = first;
        alterEgos = new(2);
        alterEgos.Add(first);
        current = (0, first);
        holdInteractions = boopInteractions = false;
        VirtualAxisInputs = ImmutableArray<VirtualInput>.Empty;

        Add(new TransitionListener() { OnOutBegin = OnTransition });
    }

    private void OnTransition()
    {
        foreach (var p in alterEgos)
        {
            if (IsOriginal(p))
                continue;
            p.RemoveSelf();
        }
    }

    public bool IsCurrent(Player player)
        => current.player == player;

    public bool IsOriginal(Player player)
        => original == player;

    public void RegisterNew(Player player)
    {
        alterEgos.Add(player);
        if (holdInteractions)
            player.Add(new AlterEgoHoldable());
        if (boopInteractions)
            player.Add(new AlterEgoBoopable());
    }

    public void Reset()
    {
        foreach (var p in alterEgos)
        {
            if (IsOriginal(p))
                continue;
            p.RemoveSelf();
        }
        RemoveSelf();
    }

    public void SetInteractions(bool hold, bool boop)
    {
        holdInteractions = hold;
        boopInteractions = boop;
        if (!holdInteractions)
            current.player.Drop();
        foreach (var player in alterEgos)
        {
            var holdable = player.Get<AlterEgoHoldable>();
            if (hold)
            {
                if (holdable is null)
                    player.Add(new AlterEgoHoldable());
            }
            else
            {
                if (holdable is not null)
                    player.Remove(holdable);
            }

            var boopable = player.Get<AlterEgoBoopable>();
            if (boop)
            {
                if (boopable is null)
                    player.Add(new AlterEgoBoopable());
            }
            else
            {
                if (boopable is not null)
                    player.Remove(boopable);
            }
        }
    }

    public bool TryRemove(Player player)
    {
        if (alterEgos.Count <= 1)
            return false;
        if (player == original)
            return false;
        if (current.player == player)
        {
            if (!TrySwitchToNext())
                return false;
        }
        alterEgos.Remove(player);
        player.RemoveSelf();
        current.index = alterEgos.IndexOf(current.player);
        if (current.index == -1)
            throw new InvalidOperationException();
        return true;
    }

    public override void Update()
    {
        base.Update();

        var btn = BitsHelperModule.Settings.SwitchBetweenPlayers;
        if (btn.Pressed)
            TrySwitchToNext();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        VirtualAxisInputs = MInput.VirtualInputs.Where(i => i is VirtualAxis or VirtualIntegerAxis).ToImmutableArray();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        VirtualAxisInputs = ImmutableArray<VirtualInput>.Empty;

        var p = original;
        p.Remove(p.Get<AlterEgoHoldable>());
        p.Remove(p.Get<AlterEgoBoopable>());
    }

    private bool TrySwitchToNext()
    {
        if (alterEgos.Count <= 1)
            return false;
        var previousPlayer = current.player;

        if (CheckBlockedOut(Scene, previousPlayer))
            return false;

        int initCurrentIndex = current.index;
        int nextIndex = (current.index + 1) % alterEgos.Count;
        Player nextPlayer = alterEgos[nextIndex];
        while (CheckBlockedIn(Scene, nextPlayer))
        {
            nextIndex++;
            nextIndex %= alterEgos.Count;
            if (nextIndex == initCurrentIndex)
                return false;
            nextPlayer = alterEgos[nextIndex];
        }

        if (nextPlayer.Scene != Scene)
        {
            // the player was removed (maybe died)
            return false;
        }

        current = (nextIndex, nextPlayer);
        var currentPlayer = current.player;

        previousPlayer.Drop();
        if (holdInteractions)
        {
            previousPlayer.Add(new AlterEgoHoldable());
            currentPlayer.Remove(currentPlayer.Get<AlterEgoHoldable>());
        }
        if (boopInteractions)
        {
            previousPlayer.Add(new AlterEgoBoopable());
            currentPlayer.Remove(currentPlayer.Get<AlterEgoBoopable>());
        }

        // makes level.Tracker.GetEntity<Player>() return the current one
        var list = Scene.Tracker.GetEntities<Player>();
        int indexOfCurrent = list.IndexOf(currentPlayer);

        Entity at0 = list[0];
        Entity atI = list[indexOfCurrent];
        list[indexOfCurrent] = at0;
        list[0] = atI;

        return true;
    }

    private static bool CheckBlockedOut(Scene scene, Player player)
    {
        foreach (AlterEgoBlockField blockField in scene.Tracker.GetEntities<AlterEgoBlockField>())
        {
            if (blockField.BlockOut && blockField.CollideCheck(player))
                return true;
        }
        return false;
    }

    private static bool CheckBlockedIn(Scene scene, Player player)
    {
        foreach (AlterEgoBlockField blockField in scene.Tracker.GetEntities<AlterEgoBlockField>())
        {
            if (blockField.BlockIn && blockField.CollideCheck(player))
                return true;
        }
        return false;
    }

    public static AlterEgoController Get(Scene scene)
        => scene.Tracker.GetEntity<AlterEgoController>();

    public static AlterEgoController GetOrCreate(Scene scene, Player player)
    {
        var controller = scene.Tracker.GetEntity<AlterEgoController>();
        if (controller is null)
        {
            controller = new(player);
            scene.Add(controller);
        }
        return controller;
    }
}
