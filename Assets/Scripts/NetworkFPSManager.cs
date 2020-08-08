using System.Collections.Generic;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;

public class NetworkFPSManager : MonoBehaviour
{
    private const float armMultiplier = 1.2f;

    private const float headMultiplier = 5;
    private const float legMultiplier = 1.1f;

    public List<Gun> gunTypes = new List<Gun>
    {
        new Gun(20, 0.75f, "0", 300, 10, 50),
        new Gun(70, 2.0f, "1", 70, 3, 200)
    };

    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    private void Shoot(Vector3 hitPosition, Vector3 hitDirection, Gun shootingGun, Player shootingPlayer, int peerId)
    {
        Player player = null;

        NetworkEntity entity = null;
        
        var hits = new RaycastHit[4];

        //Starts RaycastNonAlloc and then goes through it looking for players or entities 

        var size = Physics.RaycastNonAlloc(hitPosition, hitDirection, hits, shootingGun.Distance);

        foreach (var h in hits)
        {
            if(!h.transform) continue;

            if (h.transform.CompareTag("Player"))
            {
                var hitPlayer = h.transform.GetComponent<Player>();
                
                if (!hitPlayer.GetPeerId().Equals(peerId))
                    player = hitPlayer;
            }
            
            else if (h.transform.CompareTag("Entity")) entity = h.transform.gameObject.GetComponent<NetworkEntity>();
        }

        if (entity) EntityShoot(entity, hitDirection, hitPosition, shootingGun);

        if (!player || player.Team == shootingPlayer.Team) return;

        var playerPosition = player.transform.position; //Non possiamo passare direttamente la posizione dal transform al task! Altrimenti va in errore.

        Task.Run(() => CalculateShootData(player, playerPosition, hitPosition, shootingGun.Damage));
    }

    public void ParseShootingData(string[] data, NetPeer peer)
    {
        var hitPosition = new Vector3(float.Parse(data[1]),
            float.Parse(data[2]),
            float.Parse(data[3]));

        var hitDirection = new Vector3(float.Parse(data[4]),
            float.Parse(data[5]),
            float.Parse(data[6]));
        
        Debug.DrawRay(hitPosition, hitDirection * 10, Color.green, 100);
        
        var shootingGun = gunTypes.Find(x => x.Id == data[7]);

        var shootingPlayer = _networkManager.networkPlayer.FindPlayer(peer);
        
        _networkManager.SendMessageToClient($"PlayerShoot@{peer.Id}");
        
        Shoot(hitPosition, hitDirection, shootingGun, shootingPlayer, peer.Id);
    }

    private static void EntityShoot(NetworkEntity entity, Vector3 direction, Vector3 hitPosition, Gun gun)
    {
        var shootDistance = Vector3.Distance(entity.transform.position, hitPosition);
        entity.AddForce(direction, gun.EntityShootPower);
    }

    private void CalculateShootData(Player player, Vector3 playerPosition, Vector3 hitPosition, float damageData)
    {
        if (!player.IsAlive) return;

        var shootDistance = Vector3.Distance(playerPosition, hitPosition);

        float damage;

        if (shootDistance > 10)
            damage = damageData - shootDistance / 2; //Il danno si riduce maggiore è la distanza percorsa.

        else damage = damageData;

        player.Health -= damage;

        //Debug.Log($"Player {player.Name} received {damage} damage! {player.Health} health left");

        if (player.Health <= 0)
        {
            player.Health = 0;
            _networkManager.networkPlayer.KillPlayer(player.Name);
        }
        else
        {
            _networkManager.SendMessageToClient($"PlayerHit@{player.GetPeerId()}@{player.Health}");
        }
    }
    
    public void CalculateShootData(Player player, float damage)
    {
        if (!player.IsAlive) return;

        player.Health -= damage;

        //Debug.Log($"Player {player.Name} received {damage} damage! {player.Health} health left");

        if (player.Health <= 0)
        {
            player.Health = 0;
            _networkManager.networkPlayer.KillPlayer(player.Name);
        }
        else
        {
            _networkManager.SendMessageToClient($"PlayerHit@{player.GetPeerId()}@{player.Health}");
        }
    }
    
}