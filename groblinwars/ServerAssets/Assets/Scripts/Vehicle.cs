using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
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
              //  move.StopMovement(0.5f);
            transform.rotation = Quaternion.Euler(new Vector3(0, player.transform.GetChild(1).eulerAngles.y, 0));
            player.TryGetComponent(out Rigidbody rb);
            rb.isKinematic = true;
        }

        timer -= Time.deltaTime;
    }

    public void EnterVehicle(ushort id)
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
