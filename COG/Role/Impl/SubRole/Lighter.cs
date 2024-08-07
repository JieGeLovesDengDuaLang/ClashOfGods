﻿using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class Lighter : CustomRole, IListener
{
    public Lighter() : base(Color.yellow, CampType.Unknown, true)
    {
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerAdjustLighting(PlayerAdjustLightingEvent @event)
    {
        if (GameStates.IsLobby || !GameStates.InGame) return true;
        var player = @event.Player;
        if (IsPlayerControlRole(player)) return true;

        player.SetFlashlightInputMethod();
        player.lightSource.SetupLightingForGameplay(false, 0.75f, player.TargetFlashlight.transform);
        return false;
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Lighter();
    }
}