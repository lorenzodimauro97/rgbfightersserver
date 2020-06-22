using LiteNetLib;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
    private NetworkEntityManager _networkEntityManager;
    public string entityId;
    public string entityType;
    public Vector3 euler;
    public Vector3 position;

    private void Start()
    {
        _networkEntityManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkEntityManager>();
        _networkEntityManager.AddEntity(this);

        if (entityType != "Ammo" && entityType != "Gun")
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
        string message;

        switch (entityType)
        {
            case "Ammo":
                AmmoEntityTrigger(other);
                break;
            case "Gun":
                break;
            case "":
                CheckTriggerTagForForce(other);
                break;
        }
    }

    private void AmmoEntityTrigger(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        var peer = collider.gameObject.GetComponent<Player>().Peer;

        var message = "AmmoAdd@10";

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

    public void CheckTriggerTagForForce(Collider other)
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