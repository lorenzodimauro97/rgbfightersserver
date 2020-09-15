using System.Collections.Generic;
using System.Linq;
using Network.Messages;
using Players;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    public class Players : MonoBehaviour
    {
        public UnityInterface @interface;

        public GameObject playerObject;

        public Dictionary<uint, Player> players;

        private int _teamEteroCount, _teamRgbCount;

        private bool _teamSelect;


        private void Start()
        {
            @interface = GetComponent<UnityInterface>();
            players = new Dictionary<uint, Player>();
        }

        public void AddPlayer(uint id, string nickname)
        {
            var player = Instantiate(playerObject).GetComponent<Player>();

            player.Nickname = nickname;

            player.ID = id;

            players.Add(id, player);

            @interface.Interfaces.GameplayManager.UpdatePlayerMatchStatus(player.ID);
        }

        public void RemovePlayer(uint id)
        {
            if (!players.ContainsKey(id)) return;
            Destroy(players[id].gameObject);
            players.Remove(id);
            @interface.Interfaces.GameplayManager.SendWaitingRoomData();
        }

        public Player GetPlayer(uint id)
        {
            return players[id];
        }

        public void SpawnPlayer(uint id)
        {
            var playerColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

            var spawnPoint =
                @interface.Interfaces.GameplayManager.spawnPoints[
                    Random.Range(0, @interface.Interfaces.GameplayManager.spawnPoints.Count)];

            var newPlayer = players[id];

            _teamSelect = _teamRgbCount > _teamEteroCount;

            newPlayer.Team = _teamSelect ? "etero" : "rgb";

            if (_teamSelect) _teamEteroCount++;
            else _teamRgbCount++;

            newPlayer.SetPositionRotation(spawnPoint, quaternion.identity, quaternion.identity);

            var serializablePlayers = players.Where(p => !p.Value.ID.Equals(newPlayer.ID))
                .ToDictionary(p => p.Key, p => p.Value.Nickname);

            var message = new PlayerSpawnMessage(newPlayer.transform.position, newPlayer.Nickname, serializablePlayers,
                id);

            @interface.SendMessages(message);

            //players.Add(peer.Id.ToString(), newPlayer);

            //_networkManager.networkLeaderboard.AddPlayer(newPlayer);

            // _networkManager.SendMessageToClient(
            //     $"PlayerInformation@{peer.Id}@{newPlayer.Team}@{playerColor.r}@{playerColor.g}@{playerColor.b}@{spawnPoint.x}@{spawnPoint.y}@{spawnPoint.z}@{_networkManager.networkMap.remainingMatchSeconds}",
            //     peer);
            //
            // SendPlayerListToClients();

            //_networkManager.networkEntity.SendClientEntitiesStatus(peer);

            //_networkManager.SendChatMessage($"ChatMessage@Server:{playerData[1]} Si è Connesso!");
        }

        public void SetPlayerPositionRotation(uint id, Vector3 position, Quaternion rotation, Quaternion headRotation,
            PlayerPositionRotationMessage message)
        {
            players[id].SetPositionRotation(position, rotation, headRotation);

            message.IsBroadcast = true;

            @interface.SendMessages(message);
        }

        /*public Player.Player RemovePlayer(NetPeer peer)
        {
            if (!_networkManager.networkMap.gameplayState.Equals(1)) return null;
            var disconnectedPlayer = FindPlayer(peer);

            if (!disconnectedPlayer) return null;

            if (disconnectedPlayer.Team == "rgb") _teamRgbCount--;
            else _teamEteroCount--;

            players.Remove(peer.Id.ToString());
            _networkManager.networkLeaderboard.RemovePlayer(disconnectedPlayer);

            Destroy(disconnectedPlayer.Body);

            return disconnectedPlayer;
        }

        private void SendPlayerListToClients()
        {
            if (players.Count == 0) return;

            var message = players.Aggregate("PlayersList@", (current, player) => string.Concat(current, $"{player.Value.Name}&{player.Value.GetPeerId()}&{player.Value.Team}&{player.Value.Color.r}&{player.Value.Color.g}&{player.Value.Color.b}&{player.Value.Body.transform.position.x}&{player.Value.Body.transform.position.y}&{player.Value.Body.transform.position.z}&{player.Value.GunIndex}@"));

            _networkManager.networkLeaderboard.SendLeaderBoard();

            _networkManager.SendMessageToClient(message);
        }

        public void MovePlayer(string[] playerData, NetPeer peer)
        {
            if (!_networkManager.networkMap.gameplayState.Equals(1)) return;

            var movingPlayer = FindPlayer(peer);

            if (!movingPlayer.IsAlive) return;

            movingPlayer.NetPlayer.MovePlayer(playerData);

            SendPlayerPositionToClients(peer, playerData);
        }

        public IEnumerator KillPlayer(Player.Player deadPlayer)
        {
            deadPlayer.SetAlive(false);

            _networkManager.SendMessageToClient($"PlayerDead@{deadPlayer.GetPeerId()}");
        
            _networkManager.networkEntity.SpawnRagdoll(deadPlayer);

            yield return new WaitForSeconds(5);

            deadPlayer.SetAlive(true);

            var spawnPoint =
                _networkManager.networkMap.spawnPoints[
                    new System.Random().Next(0, _networkManager.networkMap.spawnPoints.Count)];

            _networkManager.SendMessageToClient(
                $"PlayerRespawn@{deadPlayer.GetPeerId()}@{spawnPoint.x}@{spawnPoint.y}@{spawnPoint.z}");
        }

        public Player.Player FindPlayer(NetPeer peer)
        {
            return players[peer.Id.ToString()];
        }

        public void Clear()
        {
            players.Clear();
        }

        public void ChangePlayerGunIndex(string index, NetPeer peer)
        {
            var player = FindPlayer(peer);
            player.GunIndex = index;
            _networkManager.SendMessageToClient($"GunChange@{peer.Id}@{index}");
        }

        private void SendPlayerPositionToClients(NetPeer peer, IReadOnlyList<string> playerData)
        {
            _networkManager.SendMessageToClient(
                $"PlayerPosition@{peer.Id}@{playerData[1]}@{playerData[2]}@{playerData[3]}@{playerData[4]}@{playerData[5]}@{playerData[6]}@{playerData[7]}@{playerData[8]}@{playerData[9]}@{playerData[10]}@{playerData[11]}@{playerData[12]}");
        }
    }*/
    }
}