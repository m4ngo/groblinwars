using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sped : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float defaultSpeed;
    private PlayerMovement move;

    private void Update()
    {
        if(transform.parent != null)
        {
            if (transform.parent.parent.TryGetComponent(out PlayerMovement move))
            {
                this.move = move;
                move.SetMovementSpeed(speed);
            }
            else if (move != null)
            {
                move.SetMovementSpeed(defaultSpeed);
                move = null;
            }
        } else
        {
            if (move != null)
            {
                move.SetMovementSpeed(defaultSpeed);
                move = null;
            }
        }
    }
}
