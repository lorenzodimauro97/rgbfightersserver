using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public GameObject head;
    private Player _player;
    private string ArmType;
    private string LegType;

    private void Start()
    {
        _player = GetComponent<Player>();
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

        LegType = dataArray[10];
        ArmType = dataArray[11];

        if (newPosition.y > -200 || !_player.IsAlive) return;
        StartCoroutine(GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkPlayers>()
            .KillPlayer(_player));
    }
}