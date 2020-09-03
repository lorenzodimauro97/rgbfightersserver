using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;

public class NetworkPlayers : MonoBehaviour
{
    public GameObject playerObject;

    public List<Player> players;
    private NetworkManager _networkManager;

    private bool _teamSelect;

    private int _teamRgbCount = 0;
    private int _teamEteroCount = 0;


    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();

        players = new List<Player>(_networkManager.connectedPeerLimit);
    }

    public void StartPlayer(string[] playerData, NetPeer peer)
    {
        if (players.Find(x => x.Name.Equals(playerData[1])))
            NetworkManager.DisconnectClient("Username Already in use", peer);

        Debug.Log($"Peer {playerData[1]} connesso!");

        _networkManager.networkMap.SendMatchStatus(peer);
        
    }

    public void SpawnPlayer(string[] playerData, NetPeer peer)
    {
        var playerColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        var spawnPoint =
            _networkManager.networkMap.spawnPoints[Random.Range(0, _networkManager.networkMap.spawnPoints.Count)];

        var newPlayer = Instantiate(playerObject).AddComponent<Player>();

        _teamSelect = _teamRgbCount > _teamEteroCount;
        
        newPlayer.Spawn(playerData[1], _teamSelect ? "etero" : "rgb", peer, spawnPoint, playerColor);

        if (_teamSelect) _teamEteroCount++;
        else _teamRgbCount++;

        players.Add(newPlayer);
        
        _networkManager.networkLeaderboard.AddPlayer(newPlayer);

        _networkManager.SendMessageToClient(
            $"PlayerInformation@{peer.Id}@{newPlayer.Team}@{playerColor.r}@{playerColor.g}@{playerColor.b}@{spawnPoint.x}@{spawnPoint.y}@{spawnPoint.z}@{_networkManager.networkMap.remainingMatchSeconds}",
            peer);

        SendPlayerListToClients();

        _networkManager.networkEntity.SendClientEntitiesStatus(peer);
        
        _networkManager.SendChatMessage($"ChatMessage@Server:{playerData[1]} Si è Connesso!");
    }

    public Player RemovePlayer(NetPeer peer)
    {
        var disconnectedPlayer = FindPlayer(peer);

        if (!disconnectedPlayer) return null;

            if (disconnectedPlayer.Team == "rgb") _teamRgbCount--;
        else _teamEteroCount--;
        
        players.RemoveAll(x => x.GetPeer() == peer);
        _networkManager.networkLeaderboard.RemovePlayer(disconnectedPlayer);

        return disconnectedPlayer;
    }

    private void SendPlayerListToClients()
    {
        if(players.Count == 0) return;

        var message = players.Aggregate("PlayersList@", (current, p) => current + $"{p.Name}&{p.GetPeerId()}&{p.Team}&{p.Color.r}&{p.Color.g}&{p.Color.b}&{p.Body.transform.position.x}&{p.Body.transform.position.y}&{p.Body.transform.position.z}@");
        
        _networkManager.networkLeaderboard.SendLeaderBoard();
        
        _networkManager.SendMessageToClient(message);
    }

    public void MovePlayer(string[] playerData, NetPeer peer)
    {
        if (!_networkManager.networkMap.gameplayState.Equals(1)) return;

        var movingPlayer = FindPlayer(peer);

        if (!movingPlayer.IsAlive) return;

        movingPlayer?.NetPlayer.MovePlayer(playerData);

        SendPlayerPositionToClients(peer, playerData);
    }

    public IEnumerator KillPlayer(string name)
    {
        var deadPlayer = FindPlayer(name);

        if (!deadPlayer.IsAlive) yield break;

        deadPlayer.SetAlive(false);

        _networkManager.SendMessageToClient($"PlayerDead@{deadPlayer.GetPeerId()}");

        yield return new WaitForSeconds(5);

        deadPlayer.SetAlive(true);

        var spawnPoint =
            _networkManager.networkMap.spawnPoints[
                new System.Random().Next(0, _networkManager.networkMap.spawnPoints.Count)];

        _networkManager.SendMessageToClient(
            $"PlayerRespawn@{deadPlayer.GetPeerId()}@{spawnPoint.x}@{spawnPoint.y}@{spawnPoint.z}");
    }

    public Player FindPlayer(NetPeer peer)
    {
        return players.Find(x => x.GetPeer() == peer);
    }

    public Player FindPlayer(string name)
    {
        return players.Find(x => x.Name == name);
    }

    public void Clear()
    {
        players.Clear();
        players.Capacity = 0;
    }

    public void ChangePlayerGunIndex(string index, NetPeer peer)
    {
        var player = FindPlayer(peer);
        player.SetGunIndex(index);
        _networkManager.SendMessageToClient($"GunChange@{player.GetPeerId()}@{index}");
    }

    private void SendPlayerPositionToClients(NetPeer peer, IReadOnlyList<string> playerData)
    {
        _networkManager.SendMessageToClient(
            $"PlayerPosition@{peer.Id}@{playerData[1]}@{playerData[2]}@{playerData[3]}@{playerData[4]}@{playerData[5]}@{playerData[6]}@{playerData[7]}@{playerData[8]}@{playerData[9]}@{playerData[10]}@{playerData[11]}@{playerData[12]}");
    }
}