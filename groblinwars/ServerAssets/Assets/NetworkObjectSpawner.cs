using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class NetworkObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int prefabIndex;
    [SerializeField] private bool useTrigger = true;

    private void Update()
    {
        if (!useTrigger)
        {
            NetworkObject.Spawn(prefabIndex, spawnPos.position);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger)
            return;
        if (other.CompareTag("Player"))
            NetworkObject.Spawn(prefabIndex, spawnPos.position);
    }
}
