using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private GameObject explosionEffect;

    private void OnDestroy()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }
}
