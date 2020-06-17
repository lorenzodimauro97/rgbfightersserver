using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
    public string entityType;
    public string entityId;
    
    private NetworkEntityManager _networkEntityManager;
    private Vector3 _oldPosition;
    private Vector3 _oldEuler;

    private void Start()
    {
        _networkEntityManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkEntityManager>();
        _networkEntityManager.AddEntity(this);
    }

    private void FixedUpdate()
    {
        if (_oldPosition == transform.position || _oldEuler == transform.eulerAngles) return;

        _oldPosition = transform.position;
        _oldEuler = transform.eulerAngles;

        var message = $"EntityPosition@{entityId}" +
                      $"@{_oldPosition.x}" +
                      $"@{_oldPosition.y}" +
                      $"@{_oldPosition.z}" +
                      $"@{_oldEuler.x}" +
                      $"@{_oldEuler.y}" +
                      $"@{_oldEuler.z}";
        
        SendNewEntityData(message);
    }

    private void OnTriggerEnter(Collider other)
    {
        string message;

        switch (entityType)
        {
            case "Ammo": AmmoEntityTrigger(other);
                break;
            case "Gun":
                break;
            case "":
                break;
        }
    }

    private void AmmoEntityTrigger(Collider collider)
    {
        if (!collider.CompareTag("Player")) return;

        NetPeer peer = collider.gameObject.GetComponent<Player>().Peer;
        
        var message = $"AmmoAdd@10";
        
        SendNewEntityData(message);

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
    
    public void OnCollisionEnter(Collision other)
    {
        if (!other.transform.CompareTag("Player")) return;
        var force = transform.position - other.transform.position;
        AddForce(force);
    }

    public void AddForce(Vector3 direction)
    {
        const int magnitude = 500;
        direction.Normalize();
        GetComponent<Rigidbody>().AddForce(direction * magnitude);
    }
    
    public void AddForce(Vector3 direction, float magnitude)
    {
        direction.Normalize();
        GetComponent<Rigidbody>().AddForce(direction * magnitude);
    }

    private void SendNewEntityData(string message) => _networkEntityManager.SendMessageToClient(message);
}
