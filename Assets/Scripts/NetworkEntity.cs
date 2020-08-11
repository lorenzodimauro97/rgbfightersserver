using LiteNetLib;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkEntity : MonoBehaviour
{
    public enum Entity
    {
        Ammo,
        Gun,
        Movable,
        Health,
        Damage
    }

    public Entity entityType;
    public string gunId;
    public string entityId;

    public int damageAmount;
    
    public Vector3 euler;
    public Vector3 position;

    private Rigidbody _rigidbody;
    
    private NetworkEntityManager _networkEntityManager;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _networkEntityManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkEntityManager>();
        _networkEntityManager.AddEntity(this);

        if (entityType == Entity.Movable || entityType == Entity.Damage)
            _networkEntityManager.movableEntities.Add(GetComponent<NetworkEntity>());
    }

    private void FixedUpdate()
    {
        CheckIfOutOfBounds();
        
        if (position == transform.position && euler == transform.eulerAngles) return;
        if (entityType == Entity.Health || entityType == Entity.Ammo || entityType == Entity.Gun) return;

        position = transform.position;
        euler = transform.eulerAngles;

        var message = $"EntityPosition@{name}@{entityId}" +
                      $"@{position.x}" +
                      $"@{position.y}" +
                      $"@{position.z}" +
                      $"@{euler.x}" +
                      $"@{euler.y}" +
                      $"@{euler.z}";

        SendNewEntityData(message);
    }

    private void CheckIfOutOfBounds()
    {
        if (!(transform.position.y < -1500)) return;
        
        _rigidbody.velocity = Vector3.zero;
            
        var spawnPoint =
            _networkEntityManager._networkManager.networkMap.spawnPoints[
                new System.Random().Next(0,_networkEntityManager._networkManager.networkMap.spawnPoints.Count)];
            
        transform.position = spawnPoint;
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
            case Entity.Health:
                HealthEntityTrigger(other);
                break;
            case Entity.Damage:
                DamageEntityTrigger(other);
                break;
        }
    }

    private void HealthEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;
        
        var peer = other.gameObject.GetComponent<Player>().GetPeer();

        var message = $"HealthAdd@{Random.Range(10, 70)}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), Random.Range(10, 60));

        DisableEntity();
    }

    private void GunEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;

        var peer = other.gameObject.GetComponent<Player>().GetPeer();

        var message = $"GunAdd@{gunId}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), Random.Range(20, 120));

        DisableEntity();
    }

    private void AmmoEntityTrigger(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.gameObject.GetComponent<Player>();

        var peer = player.GetPeer();

        var amount = _networkEntityManager._networkManager.networkFps.gunTypes.Find(x => x.Id == player.GetGunIndex())
            .ReloadAmount;

        var message = $"AmmoAdd@{amount}";

        SendNewEntityData(message, peer);

        Invoke(nameof(EnableEntity), 5.0f);

        DisableEntity();
    }

    private void DamageEntityTrigger(Component other)
    {
        if (!other.CompareTag("Player")) return;
        
        var player = other.gameObject.GetComponent<Player>();

        _networkEntityManager._networkManager.networkFps.CalculateShootData(player, damageAmount);
        
        SendNewEntityData($"EntityDespawn@{entityId}");

        _networkEntityManager.RemoveEntity(this);
    }

    private void DisableEntity()
    {
        gameObject.SetActive(false);

        var message = $"EntitySetActive@{entityId}@False";

        SendNewEntityData(message);
    }

    private void EnableEntity()
    {
        gameObject.SetActive(true);

        var message = $"EntitySetActive@{entityId}@True";

        SendNewEntityData(message);
    }

    private void MovableEntityTrigger(Collider other)
    {
        if (!other.transform.CompareTag("Player")) return;

        var player = other.gameObject.GetComponent<Player>();

        if (_rigidbody.velocity.magnitude > 7) _networkEntityManager._networkManager.networkFps.CalculateShootData(player, (_rigidbody.velocity.magnitude * _rigidbody.mass) / 10);

            var force = (transform.position - other.transform.position) * 2;
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