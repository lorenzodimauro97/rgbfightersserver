using LiteNetLib;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
    public enum Entity{ Ammo, Gun, Movable }

    public Entity entityType;
    public string gunId;
    public string entityId;
    
    private NetworkEntityManager _networkEntityManager;
    public Vector3 euler;
    public Vector3 position;

    private void Start()
    {
        _networkEntityManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkEntityManager>();
        _networkEntityManager.AddEntity(this);

        if (entityType != Entity.Ammo && entityType != Entity.Gun)
            _networkEntityManager.movableEntities.Add(GetComponent<NetworkEntity>());
    }

    private void FixedUpdate()
    {
        if (position == transform.position || euler == transform.eulerAngles) return;

        position = transform.position;
        euler = transform.eulerAngles;

        var message = $"EntityPosition@{entityId}" +
                      $"@{position.x}" +
                      $"@{position.y}" +
                      $"@{position.z}" +
                      $"@{euler.x}" +
                      $"@{euler.y}" +
                      $"@{euler.z}";

        SendNewEntityData(message);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (entityType)
        {
            case Entity.Ammo:
                AmmoEntityTrigger(other);
                break;
            case Entity.Gun: GunEntityTrigger(other);
                break;
            case Entity.Movable:
                CheckTriggerTagForForce(other);
                break;
        }
    }

    private void GunEntityTrigger(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        var peer = collider.gameObject.GetComponent<Player>().GetPeer();

        var message = $"GunAdd@{gunId}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), Random.Range(10, 60));

        DisableEntity();
    }

    private void AmmoEntityTrigger(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        var player = collider.gameObject.GetComponent<Player>();

        var peer = player.GetPeer();
        
        var amount = _networkEntityManager._networkManager.networkFps.gunTypes.Find(x => x.Id == player.GetGunIndex()).ReloadAmount;

        var message = $"AmmoAdd@{amount}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), 5.0f);

        DisableEntity();
    }

    private void DisableEntity()
    {
        gameObject.SetActive(false);

        var message = $"EntitySetActive@{entityId}@false";

        SendNewEntityData(message);
    }

    private void EnableEntity()
    {
        gameObject.SetActive(true);

        var message = $"EntitySetActive@{entityId}@true";

        SendNewEntityData(message);
    }

    private void CheckTriggerTagForForce(Collider other)
    {
        if (!other.transform.CompareTag("Player")) return;
        var force = transform.position - other.transform.position;
        AddForce(force);
    }

    public void OnCollisionStay(Collision other)
    {
        if (!other.transform.CompareTag("Player")) return;
        var force = transform.position - other.transform.position;
        AddForce(force);
    }

    public void AddForce(Vector3 direction)
    {
        const int magnitude = 2;
        direction.Normalize();
        GetComponent<Rigidbody>().AddForce(direction * magnitude, ForceMode.VelocityChange);
    }

    public void AddForce(Vector3 direction, float magnitude)
    {
        direction.Normalize();
        GetComponent<Rigidbody>().AddForce(direction * magnitude, ForceMode.VelocityChange);
    }

    private void SendNewEntityData(string message)
    {
        _networkEntityManager.SendMessageToClient(message);
    }

    private void SendNewEntityData(string message, NetPeer peer)
    {
        _networkEntityManager.SendMessageToClient(message, peer);
    }
}