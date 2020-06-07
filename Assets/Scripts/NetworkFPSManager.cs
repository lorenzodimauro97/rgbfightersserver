using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;

public class NetworkFPSManager : MonoBehaviour
{
    private const float armMultiplier = 1.2f;

    private const float headMultiplier = 5;
    private const float legMultiplier = 1.1f;

    //public GameObject playerObject;
    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    public void Shoot(string[] data, NetPeer peer)
    {
        var hitPosition = new Vector3(float.Parse(data[1]),
            float.Parse(data[2]),
            float.Parse(data[3]));

        var hitDirection = new Vector3(float.Parse(data[4]),
            float.Parse(data[5]),
            float.Parse(data[6]));

        _networkManager.SendMessageToClient(
            $"PlayerShoot@{peer.Id}");
        
        var hits = Physics.RaycastAll(hitPosition, hitDirection, int.Parse(data[8])).ToList();

        Player player = null;
        
        foreach (var h in hits.Where(h => h.transform.CompareTag("Player") &&
                                          !h.transform.gameObject.GetComponent<Player>().Peer.Id.Equals(peer.Id)))
        {
            player = h.transform.gameObject.GetComponent<Player>();
        }
        
        if (player == null || !player.IsAlive) return;

        var playerPosition = player.transform.position;

        //CalculateShootData(player, hitPosition, float.Parse(data[7]));

        //new Thread(() =>CalculateShootData(player, playerPosition, hitPosition, float.Parse(data[7]))).Start();
        
        Task.Run(() => CalculateShootData(player, playerPosition, hitPosition, float.Parse(data[7])));
    }

    private void CalculateShootData(Player player, Vector3 playerPosition, Vector3 hitPosition, float damageData)
    {
        var shootDistance = Vector3.Distance(playerPosition, hitPosition);

        float damage;

        if (shootDistance > 10)
            damage = damageData - shootDistance / 2; //Il danno si riduce maggiore è la distanza percorsa.

        else damage = damageData;

        player.Health -= damage;

        Debug.Log($"Player {player.Name} received {damage} damage! {player.Health} health left");

        if (player.Health <= 0)
        {
            player.Health = 0;
            _networkManager.networkPlayer.KillPlayer(player.Name);
        }
        else _networkManager.SendMessageToClient($"PlayerHit@{player.Health}", player.Peer);
    }
}