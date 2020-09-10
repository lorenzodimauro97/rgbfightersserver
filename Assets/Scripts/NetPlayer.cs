using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public GameObject head;
    private Player _player;
    private NetworkManager _networkManager;

    private void Start()
    {
        _player = GetComponent<Player>();
        _networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
    }

    public void MovePlayer(string[] dataArray)
    {
        var newPosition = new Vector3(float.Parse(dataArray[1]),
            float.Parse(dataArray[2]),
            float.Parse(dataArray[3]));

        var newEulerAngles = new Vector3(float.Parse(dataArray[4]),
            float.Parse(dataArray[5]),
            float.Parse(dataArray[6]));

        var headEulerAngles = new Vector3(float.Parse(dataArray[7]),
            float.Parse(dataArray[8]),
            float.Parse(dataArray[9]));

        transform.position = newPosition;
        transform.eulerAngles = newEulerAngles;
        head.transform.eulerAngles = headEulerAngles;

        if (newPosition.y > -200 || !_player.IsAlive) return;
        StartCoroutine(_networkManager.networkPlayer.KillPlayer(_player));
    }
}