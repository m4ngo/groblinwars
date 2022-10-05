using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnIntoBullet : MonoBehaviour
{
    [SerializeField] private int bulletIndex;
    [SerializeField] private float speed;

    private Rigidbody rb;
    private NetworkObject networkObject;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
    }

    public void Bullet()
    {
        GameObject bullet = NetworkObject.Spawn(bulletIndex, transform.position + (rb.velocity.normalized * 1.0f));
        if(bullet.TryGetComponent(out Rigidbody bulletRb))
        {
            if (bullet.TryGetComponent(out Explosive boom))
                boom.SetSafeTime(0.25f,GetClosestPlayer() );
            bulletRb.velocity = rb.velocity.normalized * speed;
            bullet.transform.rotation = transform.rotation;
            bullet.TryGetComponent(out NetworkObject obj);
            obj.lastId = networkObject.lastId;
        }
        networkObject.DestroyObject();
    }

    public Player GetClosestPlayer()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest.GetComponent<Player>();
    }
}
