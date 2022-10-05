using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrownTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent thrownEvents;

    public void Thrown()
    {
        thrownEvents.Invoke();
    }
}
