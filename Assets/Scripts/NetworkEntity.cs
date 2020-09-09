using LiteNetLib;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(Rigidbody))]
public class NetworkEntity : MonoBehaviour
{
    public enum Entity
    {
        Ammo,
        Gun,
        Movable,
        Health,
        Damage,
        Ragdoll
    }

    public Entity entityType;
    public string gunId, entityId;

    public int damageAmount;

    public Vector3 position, euler;

    public NetworkEntityManager networkEntityManager;

    private Rigidbody _rigidbody;

    public Player ownerPlayer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        networkEntityManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkEntityManager>();

        networkEntityManager.entities.Add(entityId, this);

        if (entityType == Entity.Movable || entityType == Entity.Damage || entityType == Entity.Ragdoll)
            networkEntityManager.movableEntities.Add(entityId, this);
    }

    private void FixedUpdate()
    {
        CheckIfOutOfBounds();

        var transform1 = transform;
        
        if (!networkEntityManager.networkManager.networkMap.gameplayState.Equals(1)) return;
        if (position == transform1.position && euler == transform1.eulerAngles) return;
        if (entityType == Entity.Health || entityType == Entity.Ammo || entityType == Entity.Gun) return;
        
        position = transform1.position;
        euler = transform1.eulerAngles;

        var message = $"EntityPosition@{name}@{entityId}" +
                      $"@{position.x}" +
                      $"@{position.y}" +
                      $"@{position.z}" +
                      $"@{euler.x}" +
                      $"@{euler.y}" +
                      $"@{euler.z}";

        SendNewEntityData(message);
    }

    public void OnCollisionStay(Collision other)
    {
        if (!other.transform.CompareTag("Player")) return;
        var force = transform.position - other.transform.position;
        AddForce(force);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (entityType)
        {
            case Entity.Ammo:
                AmmoEntityTrigger(other);
                break;
            case Entity.Gun:
                GunEntityTrigger(other);
                break;
            case Entity.Movable:
                MovableEntityTrigger(other);
                break;
            case Entity.Ragdoll:
                MovableEntityTrigger(other);
                break;
            case Entity.Health:
                HealthEntityTrigger(other);
                break;
            case Entity.Damage:
                DamageEntityTrigger(other);
                break;
        }
    }

    private void CheckIfOutOfBounds()
    {
        if (!(transform.position.y < -500)) return;

        _rigidbody.velocity = Vector3.zero;

        var spawnPoint =
            networkEntityManager.networkManager.networkMap.spawnPoints[
                new Random().Next(0, networkEntityManager.networkManager.networkMap.spawnPoints.Count)];

        transform.position = spawnPoint;
    }

    private void HealthEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;

        var peer = other.gameObject.GetComponent<Player>().GetPeer();

        var message = $"HealthAdd@{UnityEngine.Random.Range(10, 70)}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), UnityEngine.Random.Range(10, 60));

        DisableEntity();
    }

    private void GunEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;

        var peer = other.gameObject.GetComponent<Player>().GetPeer();

        var message = $"GunAdd@{gunId}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), UnityEngine.Random.Range(20, 120));

        DisableEntity();
    }

    private void AmmoEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.gameObject.GetComponent<Player>();

        var peer = player.GetPeer();

        var amount = networkEntityManager.networkManager.networkFps.gunTypes[player.GunIndex].ReloadAmount;

        var message = $"AmmoAdd@{amount}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), 5.0f);

        DisableEntity();
    }

    private void DamageEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.gameObject.GetComponent<Player>();

        networkEntityManager.networkManager.networkFps.CalculateShootData(player, damageAmount, ownerPlayer);

        networkEntityManager.RemoveEntity(this);
    }

    private void DisableEntity()
    {
        gameObject.SetActive(false);

        var message = $"EntitySetActive@{name}@{entityId}@False";

        SendNewEntityData(message);
    }

    private void EnableEntity()
    {
        gameObject.SetActive(true);

        var message = $"EntitySetActive@{name}@{entityId}@True";

        SendNewEntityData(message);
    }

    private void MovableEntityTrigger(Component other)
    {
        if (!other.transform.CompareTag("Player")) return;

        var player = other.gameObject.GetComponent<Player>();

        if (_rigidbody.velocity.magnitude > 7)
            networkEntityManager.networkManager.networkFps.CalculateShootData(player,
                _rigidbody.velocity.magnitude * _rigidbody.mass / 10, null);

        var force = (transform.position - other.transform.position) * 2;
        AddForce(force);
    }

    public void AddForce(Vector3 direction)
    {
        const int magnitude = 2;
        direction.Normalize();
        _rigidbody.AddForce(direction * magnitude, ForceMode.VelocityChange);
    }

    public void AddForce(Vector3 direction, float magnitude, ForceMode forceMode)
    {
        direction.Normalize();
        _rigidbody.AddForce(direction * magnitude, forceMode);
    }
    
    public void SendNewEntityData(string message)
    {
        networkEntityManager.SendMessageToClient(message);
    }

    private void SendNewEntityData(string message, NetPeer peer)
    {
        networkEntityManager.SendMessageToClient(message, peer);
    }
}