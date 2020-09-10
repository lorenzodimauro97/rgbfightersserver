using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using UnityEngine;

public class NetworkLeaderboard : MonoBehaviour
{
    public Dictionary<string, LeaderBoard> leaderBoard;
    private NetworkManager _networkManager;

    private void Start()
    {
        leaderBoard = new Dictionary<string, LeaderBoard>();
        _networkManager = GetComponent<NetworkManager>();
    }

    public void AddPlayer(Player player)
    {
        leaderBoard.Add(player.GetPeerId().ToString(), new LeaderBoard(player, 0));;
    }

    public void AddPoint(Player player)
    {
        var selectedPlayer = leaderBoard[player.GetPeerId().ToString()];

        selectedPlayer.KillCount++;

        SendLeaderBoard();
    }

    public void RemovePoint(Player player)
    {
        var selectedPlayer = leaderBoard[player.GetPeerId().ToString()];

        selectedPlayer.KillCount--;

        SendLeaderBoard();
    }
    
    public void RemovePlayer(Player player)
    {
        leaderBoard.Remove(player.GetPeerId().ToString());
    }

    public void Clear()
    {
        leaderBoard?.Clear();
    }

    public void SendLeaderBoard()
    {
        var message = leaderBoard.Aggregate("LeaderBoard@",
            (current, p) => string.Concat(current,$"{p.Value.Player.name}&{p.Value.Player.Team}&{p.Value.KillCount}@"));

        _networkManager.SendMessageToClient(message);
    }

    public void SendFinalResult()
    {
        var eteroKillCount = 0;
        var rgbKillCount = 0;

        foreach (var p in leaderBoard)
            if (p.Value.Player.Team.Contains("etero")) eteroKillCount++;
            else rgbKillCount++;

        int winningTeam;

        if (eteroKillCount > rgbKillCount) winningTeam = 1;
        else if (rgbKillCount > eteroKillCount) winningTeam = 2;
        else winningTeam = 3;

        _networkManager.SendMessageToClient(
            $"MatchResult@{eteroKillCount}@{rgbKillCount}@{winningTeam}");
    }

    public void SendFinalResult(NetPeer peer)
    {
        var eteroKillCount = 0;
        var rgbKillCount = 0;

        foreach (var p in leaderBoard)
            if (p.Value.Player.Team.Contains("etero")) eteroKillCount += p.Value.KillCount;
            else rgbKillCount += p.Value.KillCount;

        int winningTeam;

        if (eteroKillCount > rgbKillCount) winningTeam = 1;
        else if (rgbKillCount > eteroKillCount) winningTeam = 2;
        else winningTeam = 3;

        _networkManager.SendMessageToClient(
            $"MatchResult@{eteroKillCount}@{rgbKillCount}@{winningTeam}", peer);
    }
}