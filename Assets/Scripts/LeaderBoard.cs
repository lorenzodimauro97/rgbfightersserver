using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard
{
    public Player Player {get; }
    public int KillCount { get; }

    public LeaderBoard(Player player, int killCount)
    {
        Player = player;
        KillCount = killCount;
    }
}
