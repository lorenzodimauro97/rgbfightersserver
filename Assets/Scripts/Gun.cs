public class Gun
{
    public Gun(int damage, float reloadTime, string id, int distance, int reloadAmount, int entityShootPower)
    {
        Damage = damage;
        ReloadTime = reloadTime;
        Id = id;
        Distance = distance;
        ReloadAmount = reloadAmount;
        EntityShootPower = entityShootPower;
    }

    public int Damage { get; }
    public float ReloadTime { get; }
    public string Id { get; }
    public int Distance { get; }
    public int ReloadAmount { get; }
    public int EntityShootPower { get; }
}