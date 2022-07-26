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
    private float rotateAmount;
    private float rotateTimer = 0;

    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, targetDegrees, 0)), Time.deltaTime * rotateSpeed);

        if(rotateAmount != 0)
        {
            rotateTimer -= Time.deltaTime;
            if (rotateTimer <= 0)
            {
                targetDegrees += rotateAmount;
                rotateTimer = 0.2f;
            }
        }
    }

    public void Rotate(float degrees)
    {
        rotateAmount = degrees;
    }

    public void Fire()
    {
        if (!Physics.Raycast(shotPos.position, shotPos.forward, out RaycastHit hit, 500, hitMask))
            return;

        NetworkObject.Spawn(explosionId, hit.point);
    }
}
