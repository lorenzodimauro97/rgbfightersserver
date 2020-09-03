using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    private static readonly int LegType = Animator.StringToHash("LegType");
    private static readonly int ArmType = Animator.StringToHash("ArmType");

    public GameObject head;
    private Animator _animator;
    private Player _player;

    private void Start()
    {
        _animator = GetComponent<Animator>();
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

        _animator.SetInteger(LegType, int.Parse(dataArray[10]));
        _animator.SetInteger(ArmType, int.Parse(dataArray[11]));

        if (newPosition.y > -1000 || !_player.IsAlive) return;
        StartCoroutine(GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkPlayers>()
                .KillPlayer(GetComponent<Player>().name));
    }
}