using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class NetworkObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int prefabIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            NetworkObject.Spawn(prefabIndex, spawnPos.position);
    }
}
