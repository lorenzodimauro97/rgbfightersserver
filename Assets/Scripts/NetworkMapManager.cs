﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkMapManager : MonoBehaviour
{
    public NetworkManager networkManager;

    public List<Vector3> spawnPoints;

    public int gameplayState, remainingMatchSeconds;
    
    private AssetBundle _map;

    private string _mapHash, _mapPath, _mapName;
    public string mapDownloadLink;

    private int _waitingRoomTime, _gameplayTime, _leaderboardTime;

    public void StartMapManager()
    {
        _mapPath = Application.dataPath + "/Maps/map";

        _mapHash = DataFormatter.CalculateFileHash(_mapPath);

        networkManager = GetComponent<NetworkManager>();

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
        while (networkManager.netManager.IsRunning)
        {
            if (networkManager.netManager.ConnectedPeersCount < 2)
            {
                Debug.Log($"Waiting for players... connected: {networkManager.netManager.ConnectedPeersCount}");
                yield return new WaitForSeconds(5);
                gameplayState = 0;
                continue;
            }
            
            switch (gameplayState)
            {
                case 0:
                    networkManager.networkPlayer.Clear();
                    networkManager.networkEntity.Clear();
                    networkManager.networkLeaderboard.Clear();
                    yield return new WaitForSeconds(_waitingRoomTime);
                    gameplayState = 1;

                    SceneManager.LoadScene(_map.GetAllScenePaths()[0]);
                    break;

                case 1:
                    remainingMatchSeconds = _gameplayTime;
                    StartCoroutine(CountDownMatch());
                    yield return new WaitForSeconds(remainingMatchSeconds);
                    gameplayState = 2;
                    break;

                case 2:
                    Debug.Log("Match is Over, sending final result to clients...");
                    yield return new WaitForSeconds(_leaderboardTime);
                    gameplayState = 0;
                    SceneManager.LoadScene(0);
                    break;
            }
            SendMatchStatus();
        }
    }

    private void LoadSpawnPoints(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loading SpawnPoints...");

        spawnPoints = new List<Vector3>();

        var spawnPointsObjects = scene.GetRootGameObjects().Where(x => x.tag.Contains("SpawnPoint"));

        foreach (var s in spawnPointsObjects) spawnPoints.Add(s.transform.position);
    }

    public void SendMatchStatus(NetPeer peer)
    {
        switch (gameplayState)
        {
            case 0:
                networkManager.SendMessageToClient("LoadMap@2", peer);
                break;

            case 1:
                networkManager.SendMessageToClient(
                    $"DownloadMap@{mapDownloadLink}@{_mapHash}@{_mapName}@{remainingMatchSeconds}",
                    peer);
                break;
            case 2:
                networkManager.SendMessageToClient(
                    "LoadMap@3", peer);
                break;
        }
    }

    private IEnumerator CountDownMatch()
    {
        remainingMatchSeconds--;
        yield return new WaitForSeconds(1);
        if (gameplayState.Equals(1))
            StartCoroutine(CountDownMatch());
    }

    private void SendMatchStatus()
    {
        switch (gameplayState)
        {
            case 0:
                networkManager.SendMessageToClient(
                    "LoadMap@2");
                networkManager.SendMessageToClient($"WRConnectedPlayers@Connected Players:{networkManager.netManager.ConnectedPeersCount}");
                break;

            case 1:
                networkManager.SendMessageToClient(
                    $"DownloadMap@{mapDownloadLink}@{_mapHash}");
                break;
            case 2:
                networkManager.SendMessageToClient(
                    "LoadMap@3");
                break;
        }
    }

    public void PlayerMapLoaded(string[] dataArray, NetPeer peer)
    {
        switch (dataArray[2])
        {
            case "Waiting Room":
                networkManager.SendMessageToClient($"WRServerName@{networkManager.serverIntro}", peer);
                break;
            case "map":
                networkManager.networkPlayer.SpawnPlayer(dataArray, peer);
                break;
            case "LeaderBoard Room":
                networkManager.networkLeaderboard.SendFinalResult(peer);
                break;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= LoadSpawnPoints;
    }
}