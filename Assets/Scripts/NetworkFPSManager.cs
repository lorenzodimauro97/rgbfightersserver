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

        NetworkEntity entity = null;
        
        foreach (var h in hits)
        {
            if(h.transform.CompareTag("Player") && !h.transform.GetComponent<Player>().Peer.Id.Equals(peer.Id)) player = h.transform.gameObject.GetComponent<Player>();
            if (h.transform.CompareTag("Entity")) entity = h.transform.gameObject.GetComponent<NetworkEntity>();
        }

        if(entity) EntityShoot(entity, hitDirection, hitPosition);

        if (!player) return;
        Task.Run(() => CalculateShootData(player, hitPosition, data[7]));
    }

    private void EntityShoot(NetworkEntity entity, Vector3 direction, Vector3 hitPosition)
    {
        var shootDistance = Vector3.Distance(entity.transform.position, hitPosition);
        entity.AddForce(direction, 2000 / shootDistance);
    }

    private void CalculateShootData(Player player, Vector3 hitPosition, string sDamageData)
    {
        var damageData = float.Parse(sDamageData);
        
        var shootDistance = Vector3.Distance(player.transform.position, hitPosition);

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
        else _networkManager.SendMessageToClient($"PlayerHit@{player.Peer.Id}@{player.Health}");
    }
}