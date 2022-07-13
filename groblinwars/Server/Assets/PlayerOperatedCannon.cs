using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOperatedCannon : MonoBehaviour
{
    [SerializeField] private Transform shotPos;
    [SerializeField] private int explosionId;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float rotateSpeed;
    private float targetDegrees;

    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, targetDegrees, 0)), Time.deltaTime * rotateSpeed);
    }

    public void Rotate(float degrees)
    {
        targetDegrees += degrees;
    }

    public void Fire()
    {
        if (!Physics.Raycast(shotPos.position, shotPos.forward, out RaycastHit hit, 500, hitMask))
            return;

        NetworkObject.Spawn(explosionId, hit.point);
    }
}
