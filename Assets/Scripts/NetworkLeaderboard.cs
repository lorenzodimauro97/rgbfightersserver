using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
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

    public void AddPoint(Player player)
    {
        var selectedPlayer = leaderBoard.Find(p => p.Player == player);

        selectedPlayer.KillCount++;
    }

    public void RemovePlayer(Player player)
    {
        leaderBoard.RemoveAll(x => x.Player.GetPeer() == player.GetPeer());
    }

    public void Clear() => leaderBoard.Clear();

    public void SendLeaderBoard()
    {
        var message = leaderBoard.Aggregate("PlayerLeaderBoard@", (current, p) => current + $"{p.Player.name}&{p.KillCount}@");
        
        _networkManager.SendMessageToClient(message);
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
