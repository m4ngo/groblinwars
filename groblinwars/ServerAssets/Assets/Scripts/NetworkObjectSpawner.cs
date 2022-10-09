using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class NetworkObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int prefabIndex;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private Transform parent;

    private void Update()
    {
        if (!useTrigger)
        {
            if (parent != null)
                NetworkObject.Spawn(prefabIndex, spawnPos.position, parent);
            else
                NetworkObject.Spawn(prefabIndex, spawnPos.position);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger)
            return;
        if (other.CompareTag("Player"))
        {
                NetworkObject.Spawn(prefabIndex, spawnPos.position);
        }
    }
}
