using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkLeaderboard : MonoBehaviour
{
    private NetworkManager _networkManager;
    public List<LeaderBoard> leaderBoard;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
        leaderBoard = new List<LeaderBoard>();
    }

    public void AddPlayer(Player player)
    {
        leaderBoard.Add(new LeaderBoard(player, 0));
    }

    public void RemovePlayer(Player player)
    {
        leaderBoard.RemoveAll(x => x.Player.GetPeer() == player.GetPeer());
    }

    public void Clear() => leaderBoard.Clear();

    public void Send()
    {
        
    }

    public void SendFinalResult()
    {
        var eteroKillCount = 0;
        var rgbKillCount = 0;

        foreach (var p in leaderBoard)
        {
            if (p.Player.Team.Contains("etero")) eteroKillCount++;
            else rgbKillCount++;
        }

        int winningTeam;

        if (eteroKillCount > rgbKillCount) winningTeam = 1;
        else if (rgbKillCount > eteroKillCount) winningTeam = 2;
        else winningTeam = 3;
        
        _networkManager.SendMessageToClient(
            $"MatchResult@{eteroKillCount}@{rgbKillCount}@{winningTeam}");

    }
}
