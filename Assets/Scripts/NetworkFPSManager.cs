using LiteNetLib;
using UnityEngine;

public class NetworkFPSManager : MonoBehaviour
{
    private readonly float armMultiplier = 1.2f;

    private readonly float headMultiplier = 5;
    private readonly float legMultiplier = 1.1f;

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

        Physics.Raycast(hitPosition, hitDirection, out var hit, int.Parse(data[8]));

        _networkManager.SendMessageToClient(
            $"PlayerShoot@{peer.Id}@{data[9]}@{data[10]}@{data[11]}@{data[12]}@{data[13]}@{data[14]}");

        if (!hit.transform|| !hit.transform.CompareTag("Player")) return;

        var player = hit.transform.gameObject.GetComponentInParent<Animator>().gameObject;

        if (!player) return;

        var shootDistance = Vector3.Distance(player.transform.position, hitPosition);

        float damage;

        if (shootDistance > 10) damage = float.Parse(data[7]) - shootDistance / 20;    //Il danno si riduce maggiore è la distanza percorsa.

        else damage = float.Parse(data[7]);
        
        Debug.Log(damage);

        if (hit.transform.tag.Contains("Head")) damage *= headMultiplier;
        else if (hit.transform.tag.Contains("Arm")) damage *= armMultiplier;
        else if (hit.transform.tag.Contains("Leg")) damage *= legMultiplier;

        var damagedPlayer = _networkManager.networkPlayer.players.Find(x => x.Name == player.name);

        _networkManager.SendMessageToClient($"PlayerHit@{damage}",
            damagedPlayer.Peer);
    }
}