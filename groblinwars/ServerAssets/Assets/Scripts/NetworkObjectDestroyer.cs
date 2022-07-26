using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable") && other.transform.parent == null)
            other.transform.position = new Vector3(0, -5, 0);
    }
}
