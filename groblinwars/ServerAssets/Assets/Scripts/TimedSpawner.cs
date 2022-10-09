using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class TimedSpawner : MonoBehaviour
{
    public int prefabIndex;
    public float timer = 3f;

    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            NetworkObject.Spawn(prefabIndex, transform.position);
            Destroy(gameObject);
        }
    }
}
