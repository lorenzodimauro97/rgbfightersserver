using System.Collections;
using UnityEngine;

public class RGBall : MonoBehaviour
{
    private NetworkEntity _entity;

    private void Start()
    {
        _entity = GetComponent<NetworkEntity>();
        StartCoroutine(SelfDestroy(10));
    }

    private IEnumerator SelfDestroy(int time)
    {
        yield return new WaitForSeconds(time);
        _entity.networkEntityManager.RemoveEntity(_entity);
    }
}