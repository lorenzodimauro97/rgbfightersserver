using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkMapManager : MonoBehaviour
{
    public int
        gameplayState; //0 significa in attesa che siano tutti pronti (1 minuto), 1 che si sta giocando (10 minuti), 2 che è finito il gioco (30 secondi)

    public string mapDownloadLink;

    public NetworkManager networkManager;

    public List<Vector3> spawnPoints;
    private AssetBundle _map;

    private string _mapHash;

    private string _mapName;

    private string _mapPath;

    public int remainingMatchSeconds;

    public void StartMapManager()
    {
        _mapPath = Application.dataPath + "/Maps/map";

        _mapHash = DataFormatter.CalculateFileHash(_mapPath);

        networkManager = GetComponent<NetworkManager>();

        LoadMap();

        SceneManager.sceneLoaded += LoadSpawnPoints;

        StartCoroutine(MapTimer());
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
            switch (gameplayState)
            {
                case 0:
                    networkManager.networkPlayer.Clear();
                    networkManager.networkEntity.Clear();
                    yield return new WaitForSeconds(6);
                    gameplayState = 1;

                    SceneManager.LoadScene(_map.GetAllScenePaths()[0]);
                    break;

                case 1:
                    remainingMatchSeconds = 600;
                    StartCoroutine(CountDownMatch());
                    yield return new WaitForSeconds(remainingMatchSeconds);
                    gameplayState = 2;
                    break;

                case 2:
                    yield return new WaitForSeconds(3);
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

        var spawnPointsObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");

        foreach (var s in spawnPointsObjects)
        {
            Debug.Log("Loaded SpawnPoint");
            spawnPoints.Add(s.transform.position);
        }
    }

    public void SendMatchStatus(NetPeer peer)
    {
        switch (gameplayState)
        {
            case 0:
                networkManager.SendMessageToClient($"LoadMap@1", peer);
                break;

            case 1:
                networkManager.SendMessageToClient(
                    $"DownloadMap@{mapDownloadLink}@{_mapHash}@{_mapName}@{remainingMatchSeconds}",
                    peer);
                Debug.Log("Sending map to load...");
                break;
        }
    }

    IEnumerator CountDownMatch()
    {
        remainingMatchSeconds--;
        yield return new WaitForSeconds(1);
        if(gameplayState.Equals(1)) 
            StartCoroutine(CountDownMatch());
    }

    private void SendMatchStatus()
    {
        switch (gameplayState)
        {
            case 0:
                networkManager.SendMessageToClient(
                    $"LoadMap@1");
                break;

            case 1:
                networkManager.SendMessageToClient(
                    $"DownloadMap@{mapDownloadLink}@{_mapHash}");
                Debug.Log("Sending map to load to all players...");
                break;
        }
    }
}