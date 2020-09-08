public class LeaderBoard
{
    public LeaderBoard(Player player, int killCount)
    {
        Player = player;
        KillCount = killCount;
    }

    public Player Player { get; }
    public int KillCount { get; set; }
}