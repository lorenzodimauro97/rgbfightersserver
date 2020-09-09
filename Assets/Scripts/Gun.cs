public class Gun
{
    public Gun(int damage, float reloadTime, int distance, int reloadAmount, int entityShootPower,
        int entityBulletPower)
    {
        Damage = damage;
        ReloadTime = reloadTime;
        Distance = distance;
        ReloadAmount = reloadAmount;
        EntityShootPower = entityShootPower;
        EntityBulletPower = entityBulletPower;
    }

    public int Damage { get; }
    public float ReloadTime { get; }
    public int Distance { get; }
    public int ReloadAmount { get; }
    public int EntityShootPower { get; }

    public int EntityBulletPower { get; }
}