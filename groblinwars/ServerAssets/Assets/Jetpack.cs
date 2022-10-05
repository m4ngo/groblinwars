using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
    [SerializeField] private float upForce;
    private Rigidbody rb;

    private void Update()
    {
        if (transform.parent != null)
        {
            if (transform.parent.parent.TryGetComponent(out Rigidbody rb))
            {
                this.rb = rb;
                rb.AddForce(Vector3.up * upForce);
            }
            else if (rb != null)
            {
                rb = null;
            }
        }
        else
        {
            if (rb != null)
            {
                rb = null;
            }
        }
    }
}
