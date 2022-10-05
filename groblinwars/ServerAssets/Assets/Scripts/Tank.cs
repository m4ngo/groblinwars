using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] private Transform shotPos;
    [SerializeField] private int cannonBallIndex;

    [SerializeField] private float cannonballSpeed;
    [SerializeField] private Transform playerHolder;
    [SerializeField] private GameObject player;

    private float timer = 0f;

    private void Update()
    {
        if (playerHolder.childCount <= 0)
        {
            if (player != null)
            {
                player.TryGetComponent(out Rigidbody rb);
                rb.isKinematic = false;
                player.transform.position = transform.position + new Vector3(0, 2, 0);
                player.transform.localEulerAngles = Vector3.zero;
            }
            player = null;
        }
        else
        {
            player.transform.localPosition = Vector3.zero;
           // if (player.TryGetComponent(out PlayerMovement move))
                //move.StopMovement(0.5f);
            transform.rotation = player.transform.GetChild(1).rotation;
            player.TryGetComponent(out Rigidbody rb);
            rb.isKinematic = true;
        }

        timer -= Time.deltaTime;
    }

    public void Fire()
    {
        if (timer > 0)
            return;

        GameObject cannonball = NetworkObject.Spawn(cannonBallIndex, shotPos.position);
        cannonball.GetComponent<Rigidbody>().velocity = transform.forward * cannonballSpeed;
        timer = 1.0f;
    }

    public void EnterTank(ushort id)
    {
        if (player == null)
        {
            Player.list.TryGetValue(id, out Player temp);
            player = temp.gameObject;
            player.transform.SetParent(playerHolder);
            player.transform.localPosition = Vector3.zero;
            if (player.TryGetComponent(out PlayerMovement move))
                move.StopMovement(0.5f);
        }
    }
}
