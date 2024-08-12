﻿using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner.Data;
using COG.Role;
using COG.Utils;

namespace COG.Game.CustomWinner.Winnable;

public class CrewmatesCustomWinner : IWinnable
{
    public void CheckWin(WinnableData data)
    {
        var taskComplete = PlayerUtils.GetAllPlayers().All(player => player.Data.IsIncomplete);
        if (taskComplete ||
            PlayerUtils.AllImpostors.Where(pair => pair.Player && !pair.Player.Data.IsDead)
                .Select(pair => pair.Player).ToList().Count <= 0 
            && PlayerUtils.GetAllAlivePlayers().Select(p => p.GetMainRole()).Where(role => role.CanKill).ToArray().Length <= 0)
        {
            data.GameOverReason = taskComplete ? GameOverReason.HumansByTask : GameOverReason.HumansByVote;
            data.WinnableCampType = CampType.Crewmate;
            data.WinnablePlayers.AddRange(PlayerUtils.AllCrewmates.Select(p => p.Player));
            data.WinText = LanguageConfig.Instance.CrewmatesWinText;
            data.WinColor = Palette.CrewmateBlue;
            data.Winnable = true;
        }
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(2);
    }
}