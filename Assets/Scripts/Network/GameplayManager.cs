using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network.Messages;
using Players;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class GameplayManager : MonoBehaviour
    {
        public List<Vector3> spawnPoints;

        public int gameplayState, remainingMatchSeconds;
    
        private AssetBundle _map;

        private string _mapHash, _mapPath, _mapName, _serverIntro;
        private string _mapDownloadLink;

        private int _waitingRoomTime, _gameplayTime, _leaderboardTime, _minimumPlayers;
        
        public UnityInterface @interface;

        public void StartMapManager()
        {
            @interface = GetComponent<UnityInterface>();
            
            _mapPath = Application.dataPath + "/Maps/map";

            _mapHash = DataFormatter.CalculateFileHash(_mapPath);

            _serverIntro = ConfigParser.GetValueString("serverIntro");

            _mapDownloadLink = $"http://{ConfigParser.GetValueString("mapDownloadLink")}";

            LoadMap();

            LoadConfigData();
        
            SceneManager.sceneLoaded += LoadSpawnPoints;

            StartCoroutine(MapTimer());
        }

        private void LoadConfigData()
        {
            _waitingRoomTime = ConfigParser.GetValueInt("waitingRoomTime");
            _gameplayTime = ConfigParser.GetValueInt("gameplayTime");
            _leaderboardTime = ConfigParser.GetValueInt("leaderboardTime");
            _minimumPlayers = ConfigParser.GetValueInt("minimumPlayers");
        }

        private void LoadMap()
        {
            Debug.Log("Adding map to server...");
            _map = AssetBundle.LoadFromFile(_mapPath);
            if (_map != null) return;
            Debug.LogError("Map not found! quitting...");
            Application.Quit();
        }

        private IEnumerator MapTimer()
        {
            gameplayState = 2;
            yield return new WaitForSeconds(0.25f);
            while (!@interface.Server._isQuitting)
            {
                if (@interface._interfaces.Players.players.Count < _minimumPlayers)
                {
                    Debug.Log($"Waiting for players... connected: {@interface._interfaces.Players.players.Count}");
                    yield return new WaitForSeconds(5);
                    gameplayState = 2;
                    continue;
                }
            
                switch (gameplayState)
                {
                    case 2:
                        yield return new WaitForSeconds(_waitingRoomTime);
                        gameplayState = 1;
                        SceneManager.LoadScene(_map.GetAllScenePaths()[0]);
                        UpdatePlayerMatchStatus();
                        break;

                    case 1:
                        remainingMatchSeconds = _gameplayTime;
                        StartCoroutine(CountDownMatch());
                        yield return new WaitForSeconds(remainingMatchSeconds);
                        gameplayState = 3;
                        break;

                    case 3:
                        Debug.Log("Match is Over, sending final result to clients...");
                        yield return new WaitForSeconds(_leaderboardTime);
                        gameplayState = 2;
                        SceneManager.LoadScene(1);
                        break;
                }
            }
        }

        public void UpdatePlayerMatchStatus(uint id)
        {
            var message = new LoadMapMessage(gameplayState, _mapDownloadLink, 
                _mapHash, false, id);
            @interface.SendMessages(message);
        }

        private void UpdatePlayerMatchStatus()
        {
            var message = new LoadMapMessage(gameplayState, _mapDownloadLink, 
                _mapHash, true, 0);
            @interface.SendMessages(message);
        }

        public void SendWaitingRoomData()
        {
            var message = new WaitingRoomMessage(@interface._interfaces.Players.players.Count, _minimumPlayers, _serverIntro, true);
            @interface.SendMessages(message);
        }

        public void PlayerLoadedMap(int index, uint peerID)
        {
            Debug.Log(index);
            switch (index)
            {
                case -1: @interface._interfaces.Players.SpawnPlayer(peerID);
                    break;
                case 2: SendWaitingRoomData();
                    break;
            }
        }

        private void LoadSpawnPoints(Scene scene, LoadSceneMode mode)
        {
            if(!scene.name.Contains("map")) return;

            Debug.Log("Loading SpawnPoints...");

            spawnPoints = new List<Vector3>();

            var spawnPointsObjects = scene.GetRootGameObjects().Where(x => x.tag.Contains("SpawnPoint"));

            foreach (var s in spawnPointsObjects) spawnPoints.Add(s.transform.position);
        }

        private IEnumerator CountDownMatch()
        {
            remainingMatchSeconds--;
            yield return new WaitForSeconds(1);
            if (gameplayState.Equals(1))
                StartCoroutine(CountDownMatch());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= LoadSpawnPoints;
        }
    }
}