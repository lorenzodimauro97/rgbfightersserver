public class Gun
{
    public Gun(int damage, float reloadTime, string id, int distance, int reloadAmount, int entityShootPower, int entityBulletPower)
    {
        Damage = damage;
        ReloadTime = reloadTime;
        Id = id;
        Distance = distance;
        ReloadAmount = reloadAmount;
        EntityShootPower = entityShootPower;
        EntityBulletPower = entityBulletPower;
    }

    public int Damage { get; }
    public float ReloadTime { get; }
    public string Id { get; }
    public int Distance { get; }
    public int ReloadAmount { get; }
    public int EntityShootPower { get;}
    
    public int EntityBulletPower { get;}
}