using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class NetworkObject : MonoBehaviour
{
    public static Dictionary<ushort, NetworkObject> list = new Dictionary<ushort, NetworkObject>();
    public static ushort currentIndex;

    public ushort Id { get; private set; }
    public int PrefabIndex { get; private set; }

    public int lastId = -1;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -5f && transform.parent == null)
            DestroyObject();
        SendMovement();

        if(TryGetComponent(out Rigidbody rb))
        {
            if (rb.velocity.magnitude <= 5)
                lastId = -1;
        }
    }

    public static GameObject Spawn(int prefabIndex, Vector3 position)
    {
        ushort id = Convert.ToUInt16(currentIndex);
        currentIndex++;

        /*if(list.Count > 0)
        {
            foreach (NetworkObject otherObject in list.Values)
                otherObject.SendSpawned(id);
        }*/

        NetworkObject networkObject = Instantiate(GameLogic.Singleton.NetworkPrefabs[prefabIndex], position, Quaternion.identity).GetComponent<NetworkObject>();
        networkObject.Id = id;
        networkObject.PrefabIndex = prefabIndex;
        print(Convert.ToInt32(id));

        networkObject.SendSpawned();
        list.Add(id, networkObject);

        return networkObject.gameObject;
    }

    public static GameObject Spawn(int prefabIndex, Vector3 position, Transform parent)
    {
        ushort id = Convert.ToUInt16(currentIndex);
        currentIndex++;

        /*if(list.Count > 0)
        {
            foreach (NetworkObject otherObject in list.Values)
                otherObject.SendSpawned(id);
        }*/

        NetworkObject networkObject = Instantiate(GameLogic.Singleton.NetworkPrefabs[prefabIndex], position, Quaternion.identity).GetComponent<NetworkObject>();
        networkObject.transform.SetParent(parent);
        print(networkObject.transform.parent);
        networkObject.Id = id;
        networkObject.PrefabIndex = prefabIndex;
        print(Convert.ToInt32(id));

        networkObject.SendSpawned();
        list.Add(id, networkObject);

        return networkObject.gameObject;
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.networkObjectSpawned)));
    }

    public void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.networkObjectSpawned)), toClientId);
    }

    public void DestroyObject()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.networkObjectDestroyed);
        message.AddUShort(Id);
        NetworkManager.Singleton.Server.SendToAll(message);

        Destroy(this.gameObject);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddInt(PrefabIndex);
        message.AddVector3(transform.position);
        return message;
    }

    private void SendMovement()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
            return;

        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.networkObjectMovement);
        message.AddUShort(Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    #endregion
}
