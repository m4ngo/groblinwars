using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Linq;
using System;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != null)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField IPField;

    [Space]

    [Header("Customize")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject[] baseObj;
    [SerializeField] private GameObject shirtObj;
    [SerializeField] private GameObject backpackObj;

    [SerializeField] private string[] colors;
    [SerializeField] private int hat;
    [SerializeField] private Transform hatHolder;
    private GameObject currentHat;

    [Space]

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private InputField sensitivityField;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if (connectUI.activeInHierarchy)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P))
                pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
        }
    }

    public float GetSensitivity()
    {
        return float.Parse(sensitivityField.text);
    }

    public bool GetPaused()
    {
        return pauseMenu.activeInHierarchy;
    }

    public void ConnectClicked()
    {
        if (!IsValidIPAddress(IPField.text))
            return;

        usernameField.interactable = false;
        connectUI.SetActive(false);
        playerModel.SetActive(false);

        NetworkManager.Singleton.Connect(IPField.text);
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
        playerModel.SetActive(true);
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        message.AddStrings(colors);
        message.AddInt(hat);
        NetworkManager.Singleton.Client.Send(message);
    }
    public bool IsValidIPAddress(string IpAddress)
    {
        IPAddress IP;
        if (IpAddress.Count(c => c == '.') == 3)
        {
            bool flag = IPAddress.TryParse(IpAddress, out IP);
            if (flag)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void SetBaseColor(string color)
    {
        ColorUtility.TryParseHtmlString(color, out Color outColor);
        foreach (GameObject obj in baseObj)
        {
            obj.GetComponent<MeshRenderer>().material.color = outColor;
        }
        colors[0] = color;
    }
    public void SetShirtColor(string color)
    {
        ColorUtility.TryParseHtmlString(color, out Color outColor);
        shirtObj.GetComponent<MeshRenderer>().material.color = outColor;
        colors[1] = color;
    }
    public void SetBackpackColor(string color)
    {
        ColorUtility.TryParseHtmlString(color, out Color outColor);
        backpackObj.GetComponent<MeshRenderer>().material.color = outColor;
        colors[2] = color;
    }

    public void SetHat(int index)
    {
        hat = index;
        if(currentHat != null)
            Destroy(currentHat);
        if(hat != -1)
        {
            currentHat = Instantiate(GameLogic.Singleton.Hats[hat], hatHolder.position, hatHolder.rotation);
            currentHat.transform.SetParent(hatHolder);
        }
    }
}
