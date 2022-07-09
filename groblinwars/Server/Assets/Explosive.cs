using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private float velocityThreshold;

    [SerializeField] private float force;
    [SerializeField] private float radius;
    [SerializeField] private float stunTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement move))
            return;

        if (other.TryGetComponent(out Rigidbody otherRb))
        {
            if (otherRb.velocity.magnitude > velocityThreshold)
                StartCoroutine(Explode());
        }

        if (rb.velocity.magnitude > velocityThreshold)
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in colliders)
        {
            if (hit.TryGetComponent(out Rigidbody rb))
                rb.velocity = (hit.transform.position - transform.position).normalized * force /(Mathf.Max(1,Mathf.Sqrt(Vector3.Distance(transform.position, hit.transform.position))));
            if (hit.TryGetComponent(out PlayerMovement move))
                move.StopMovement(stunTime);
        }

        yield return new WaitForEndOfFrame();

        networkObject.DestroyObject();
    }
}
