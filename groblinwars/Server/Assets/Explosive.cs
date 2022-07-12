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

    private float safeTime = 0;
    private Player player;

    private void Update()
    {
        safeTime -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(safeTime > 0 && collision.collider.TryGetComponent(out Player otherPlayer))
        {
            if (otherPlayer == player)
                return;
        }

        if (collision.collider.TryGetComponent(out Rigidbody otherRb))
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
                rb.velocity = rb.velocity + (hit.transform.position - transform.position).normalized * force /(Mathf.Max(1,Mathf.Sqrt(Vector3.Distance(transform.position, hit.transform.position))));
            if (hit.TryGetComponent(out PlayerMovement move))
                move.StopMovement(stunTime);
        }

        yield return new WaitForEndOfFrame();

        networkObject.DestroyObject();
    }

    public void SetSafeTime(float time, Player id) 
    {
        safeTime = time;
        player = id;
    }
}
