using System.Collections.Generic;
using LiteNetLib;
using Unity.Mathematics;
using UnityEngine;

public class NetworkFPSManager : MonoBehaviour
{
    private const float armMultiplier = 1.2f;

    private const float headMultiplier = 5;
    private const float legMultiplier = 1.1f;

    public List<Gun> gunTypes = new List<Gun>
    {
        new Gun(20, 0.75f, "0", 300, 10, 50, 0),
        new Gun(70, 2.0f, "1", 70, 3, 200, 60),
        new Gun(15, 0.2f, "2", 350, 50, 75, 0)
    };

    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    public void ParseEntityShootData(string[] data, NetPeer peer)
    {
        var player = _networkManager.networkPlayer.FindPlayer(peer);
        
        var hitPosition = new Vector3(float.Parse(data[1]),
            float.Parse(data[2]),
            float.Parse(data[3]));

        var hitDirection = new Vector3(float.Parse(data[4]),
            float.Parse(data[5]),
            float.Parse(data[6]));

        EntityShoot(hitPosition, hitDirection, data[7], data[8], player);
    }

    private void EntityShoot(Vector3 hitPosition, Vector3 hitDirection, string gunIndex, string entityName, Player player)
    {
        var gun = gunTypes.Find(x => x.Id == gunIndex);

        var entity = _networkManager.networkEntity.SpawnEntity(hitPosition, Quaternion.identity, entityName, player);

        entity.GetComponent<Rigidbody>().AddForce(hitDirection * gun.EntityBulletPower, ForceMode.VelocityChange);
    }

    private void Shoot(Vector3 hitPosition, Vector3 hitDirection, Gun shootingGun, Player shootingPlayer, int peerId)
    {
        var hits = new RaycastHit[6];

        Physics.RaycastNonAlloc(hitPosition, hitDirection, hits, shootingGun.Distance);

        foreach (var h in hits)
        {
            if (!h.transform || !h.transform.CompareTag("Player")) continue;
            
            if (h.transform.CompareTag("Player"))
            {
                var hitPlayer = h.transform.GetComponent<Player>();

                if (hitPlayer.Team == shootingPlayer.Team || hitPlayer.GetPeerId() == peerId) return;

                CalculateShootData(shootingPlayer, hitPlayer, shootingGun.Damage);
            }

            else if (h.transform.CompareTag("Entity"))
            {
                EntityShoot(h.transform.gameObject.GetComponent<NetworkEntity>(), hitDirection, shootingGun);
            }
        }
    }

    public void ParseShootingData(string[] data, NetPeer peer)
    {
        var hitPosition = new Vector3(float.Parse(data[1]),
            float.Parse(data[2]),
            float.Parse(data[3]));

        var hitDirection = new Vector3(float.Parse(data[4]),
            float.Parse(data[5]),
            float.Parse(data[6]));

        var shootingGun = gunTypes.Find(x => x.Id == data[7]);

        var shootingPlayer = _networkManager.networkPlayer.FindPlayer(peer);

        _networkManager.SendMessageToClient($"PlayerShoot@{peer.Id}");

        Shoot(hitPosition, hitDirection, shootingGun, shootingPlayer, peer.Id);
    }

    private static void EntityShoot(NetworkEntity entity, Vector3 direction, Gun gun)
    {
        entity.AddForce(direction, gun.EntityShootPower, ForceMode.VelocityChange);
    }

    private void CalculateShootData(Player shootingPlayer, Player player, float damage)
    {
        if (!player.IsAlive) return;

        player.Health -= damage;

        if (player.Health <= 0)
        {
            player.Health = 0;
            _networkManager.networkLeaderboard.AddPoint(shootingPlayer);
            StartCoroutine(_networkManager.networkPlayer.KillPlayer(player.Name));
        }
        else
        {
            _networkManager.SendMessageToClient($"PlayerHit@{player.GetPeerId()}@{player.Health}");
        }
    }

    public void CalculateShootData(Player player, float damage, Player shootingPlayer)
    {
        if (!player.IsAlive || shootingPlayer == player) return;

        player.Health -= damage;

        if (player.Health <= 0)
        {
            player.Health = 0;
            
            StartCoroutine(_networkManager.networkPlayer.KillPlayer(player.Name));

            if (!shootingPlayer) return;
            
            if (player.Team != shootingPlayer.Team) _networkManager.networkLeaderboard.AddPoint(shootingPlayer);
            else _networkManager.networkLeaderboard.RemovePoint(shootingPlayer);
        }
        else
        {
            _networkManager.SendMessageToClient($"PlayerHit@{player.GetPeerId()}@{player.Health}");
        }
    }
}