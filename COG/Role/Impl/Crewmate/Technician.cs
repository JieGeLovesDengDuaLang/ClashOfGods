using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.VentImpl;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.Utils;
using COG.Utils.Coding;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

// TODO: Fix vent outline
public class Technician : CustomRole, IListener
{
    public Technician() : base(Palette.Orange, CampType.Crewmate)
    {
        CanVent = true;

        RepairButton = CustomButton.Of(() =>
        {
            RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.ClearSabotages).Finish();
            RepairSabotages();
        },
        () => RepairButton.ResetCooldown(),
        () => PlayerControl.LocalPlayer.myTasks.ToArray().Any(t => PlayerTask.TaskIsEmergency(t)),
        () => true,
        ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.Buttons.Repair.png")!, // IDK WHY THE SPRITE CANT BE SHOWN
        2,
        KeyCode.R,
        LanguageConfig.Instance.RepairAction,
        () => 0f,
        2
        );
        
        AddButton(RepairButton);
    }

    private CustomButton RepairButton { get; }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnVentCheck(VentCheckEvent @event)
    {
        var data = @event.PlayerInfo;
        var player = data.Object;
        var vent = @event.Vent;
        if (!IsLocalPlayerRole(player)) return;
        if (PlayerControl.LocalPlayer.AllTasksCompleted())
        {
            HudManager.Instance.ImpostorVentButton.Show();

            var dist = float.MaxValue;
            var couldUse = (!player.MustCleanVent(vent.Id) || (player.inVent && Vent.currentVent == vent))
                && !data.IsDead && (player.CanMove || player.inVent);
            if (ShipStatus.Instance.Systems.TryGetValueSafeIl2Cpp(SystemTypes.Ventilation, out var systemType))
            {
                var ventilationSystem = systemType.Cast<VentilationSystem>();
                if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id))
                    couldUse = false;
            }
            var canUse = couldUse;
            if (canUse)
            {
                var center = player.Collider.bounds.center;
                var position = vent.transform.position;
                dist = Vector2.Distance(center, position);
                canUse &= dist <= vent.UsableDistance
                    && !PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask, false);
            }

            @event.SetCanUse(canUse);
            @event.SetCouldUse(couldUse);
            @event.SetResult(dist);
        }
        else
        {
            @event.SetCanUse(false);
            @event.SetCouldUse(false);
            @event.SetResult(float.MaxValue);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnRpcReceived(PlayerHandleRpcEvent @event)
    {
        if ((KnownRpc)@event.CallId == KnownRpc.ClearSabotages)
            RepairSabotages();
    }

    public void RepairSabotages()
    {
        var ship = ShipStatus.Instance;
        var mapId = (MapNames)(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay ?
        AmongUsClient.Instance.TutorialMapId :
        GameUtils.GetGameOptions().MapId);

        ship.RepairCriticalSabotages();
        if (ship.Systems.TryGetValueSafeIl2Cpp(SystemTypes.Electrical, out var system))
        {
            var elecSystem = system.TryCast<SwitchSystem>();
            if (elecSystem != null)
                elecSystem.ActualSwitches = elecSystem.ExpectedSwitches;
        }

        ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 0);
        ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 1);

        if (mapId != MapNames.Mira)
            ship.AllDoors.ForEach(d => d.SetDoorway(true));

        if (mapId == MapNames.Fungle)
        {
            var mixup = ship.Cast<FungleShipStatus>().specialSabotage;
            if (mixup.IsActive)
                mixup.currentSecondsUntilHeal = 0.1f;
        }
    }

    public override IListener GetListener()
    {
        return this;
    }
}