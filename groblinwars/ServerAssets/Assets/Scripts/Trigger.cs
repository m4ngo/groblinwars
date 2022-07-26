using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerExit;
    [SerializeField] private string[] tags;

    private void OnTriggerEnter(Collider other)
    {
        foreach (string tag in tags)
        {
            if (other.CompareTag(tag))
                onTriggerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (string tag in tags)
        {
            if (other.CompareTag(tag))
                onTriggerExit.Invoke();
        }
    }
}
