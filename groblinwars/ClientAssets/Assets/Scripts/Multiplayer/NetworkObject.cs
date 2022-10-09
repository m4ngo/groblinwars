using Riptide;
using Riptide.Utils;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class NetworkObject : MonoBehaviour
{
    public static Dictionary<ushort, NetworkObject> list = new Dictionary<ushort, NetworkObject>();

    public ushort Id { get; private set; }

    public bool showGraphics = true;
    [SerializeField] private MeshRenderer graphic;
    [SerializeField] private Collider[] collider;
    [SerializeField] private Interpolator interpolator;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, int prefabIndex, Vector3 position)
    {
        NetworkObject networkObject = Instantiate(GameLogic.Singleton.NetworkPrefabs[prefabIndex], position, Quaternion.identity).GetComponent<NetworkObject>();
        networkObject.Id = id;
        print(Convert.ToInt32(id));
        list.Add(id, networkObject);
    }

    private void Move(ushort tick, Vector3 newPosition, Quaternion rotation)
    {
        if(graphic != null)
            graphic.enabled = showGraphics;
        foreach (Collider col in collider)
            col.enabled = showGraphics;

        interpolator.NewUpdate(tick, newPosition);
        transform.rotation = rotation;
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.networkObjectSpawned)]
    private static void SpawnNetworkObject(Message message)
    {
        Spawn(message.GetUShort(), message.GetInt(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.networkObjectMovement)]
    private static void MoveNetworkObject(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out NetworkObject networkObject))
            networkObject.Move(message.GetUShort(), message.GetVector3(), message.GetQuaternion());
    }
    #endregion
}
