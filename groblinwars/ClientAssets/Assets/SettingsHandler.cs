using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsHandler : MonoBehaviour
{
    public float aimSensitivity;
    public float masterVolume;
    public float musicVolume;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("SettingsHandler");

        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
