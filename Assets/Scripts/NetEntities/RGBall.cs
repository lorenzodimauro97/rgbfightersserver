using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBall : MonoBehaviour
{
    private NetworkEntity _entity;
    void Start()
    {
        _entity = GetComponent<NetworkEntity>();
        StartCoroutine(SelfDestroy(10));
    }

    IEnumerator SelfDestroy(int time)
    {
        yield return new WaitForSeconds(time);
        _entity._networkEntityManager.RemoveEntity(_entity);
        _entity.SendNewEntityData($"EntityDespawn@{_entity.entityId}");
        Destroy(gameObject);
    }
}
