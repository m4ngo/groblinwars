using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject customizationMenu;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject[] baseObj;
    [SerializeField] private GameObject shirtObj;
    [SerializeField] private GameObject backpackObj;

    [SerializeField] private TMP_InputField baseColorInput;
    [SerializeField] private TMP_InputField shirtColorInput;
    [SerializeField] private TMP_InputField backpackColorInput;

    [SerializeField] private string[] colors;
    [SerializeField] private int hat;
    [SerializeField] private Transform hatHolder;
    private GameObject currentHat;

    [Space]

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private InputField sensitivityField;
    [SerializeField] private InputField menuSensField;

    [Space]

    [Header("Death")]
    [SerializeField] private GameObject deathScreen;
    public TMP_Text winText;
    private float winTextDelay;

    private SettingsHandler handler;

    private void Awake()
    {
        Singleton = this;
        baseColorInput.onEndEdit.AddListener(delegate { SetBaseColor(baseColorInput.text) ; });
        shirtColorInput.onEndEdit.AddListener(delegate { SetShirtColor(shirtColorInput.text); });
        backpackColorInput.onEndEdit.AddListener(delegate { SetBackpackColor(backpackColorInput.text); });
    }

    private void Start()
    {
        handler = GameObject.FindGameObjectWithTag("SettingsHandler").GetComponent<SettingsHandler>();
        menuSensField.text = handler.aimSensitivity + "";
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
            if (Input.GetKeyDown(KeyCode.Escape))
                pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
        }

        if (menuSensField.gameObject.activeInHierarchy)
        {
            sensitivityField.text = menuSensField.text;
        }
        handler.aimSensitivity = float.Parse(sensitivityField.text);

        winTextDelay -= Time.deltaTime;
        if (winTextDelay <= 0)
        {
            winText.text = "";
        }
    }

    public void SetDeathScreen(bool active) { deathScreen.SetActive(active); }

    public float GetSensitivity()
    {
        return handler.aimSensitivity;
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

    public void DisconnectClicked()
    {
        handler.aimSensitivity = float.Parse(sensitivityField.text);
        NetworkManager.Singleton.Client.Disconnect();
        BackToMain();
        SceneManager.LoadScene(0);
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
        pauseMenu.SetActive(false);
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
        if (color.Length < 6) return;
        if (!color.StartsWith("#")) color = "#" + color;
                 
        ColorUtility.TryParseHtmlString(color, out Color outColor);
        foreach (GameObject obj in baseObj)
        {
            obj.GetComponent<MeshRenderer>().material.color = outColor;
        }
        colors[0] = color;
        baseColorInput.text = color.Remove(0,1);
    }
    public void SetShirtColor(string color)
    {
        if (color.Length < 6) return;
        if (!color.StartsWith("#")) color = "#" + color;

        ColorUtility.TryParseHtmlString(color, out Color outColor);
        shirtObj.GetComponent<MeshRenderer>().material.color = outColor;
        colors[1] = color;
        shirtColorInput.text = color.Remove(0, 1);
    }
    public void SetBackpackColor(string color)
    {
        if (color.Length < 6) return;
        if (!color.StartsWith("#")) color = "#" + color;

        ColorUtility.TryParseHtmlString(color, out Color outColor);
        backpackObj.GetComponent<MeshRenderer>().material.color = outColor;
        colors[2] = color;
        backpackColorInput.text = color.Remove(0, 1);
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

    public void ToggleCustomizationMenu()
    {
        customizationMenu.SetActive(!customizationMenu.activeInHierarchy);
    }

    public void ToggleSettingsMenu()
    {
        settingsMenu.SetActive(!settingsMenu.activeInHierarchy);
    }

    public void SetWinScreen(string name)
    {
        winText.text = name + " won!";
        winTextDelay = 5f;
    }
}
