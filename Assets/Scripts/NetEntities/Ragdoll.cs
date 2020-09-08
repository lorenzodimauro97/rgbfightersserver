using System.Collections;
using UnityEngine;

namespace NetEntities
{
    public class Ragdoll : MonoBehaviour
    {
        private NetworkEntity _entity;
        private void Start()
        {
            _entity = GetComponent<NetworkEntity>();
            StartCoroutine(SelfDestroy(180));
        }

        private IEnumerator SelfDestroy(int time)
        {      
            yield return new WaitForSeconds(time);
            _entity.networkEntityManager.RemoveEntity(_entity);
        }
    }
}