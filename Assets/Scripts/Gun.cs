using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun
{

    public int Damage { get; }
    public float ReloadTime { get; }
    public string Id { get; }
    public int Distance { get; }
    public int ReloadAmount { get; }


    public Gun(int damage, float reloadTime, string id, int distance, int reloadAmount)
    {
        Damage = damage;
        ReloadTime = reloadTime;
        Id = id;
        Distance = distance;
        ReloadAmount = reloadAmount;
    }
}
