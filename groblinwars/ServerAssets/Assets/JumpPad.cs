using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private Vector3 boostDir;
    [SerializeField] private float boostSpeed;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Rigidbody rb))
            return;
        rb.AddForce(boostDir * boostSpeed, ForceMode.Impulse);
    }
}
