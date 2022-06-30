using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform camTransform;
    [SerializeField] private Interpolator interpolator;
    [SerializeField] private TextMeshPro nameTag;
    private CameraController camController;

    [Header("Customize")]
    [SerializeField] private GameObject[] baseObj;
    [SerializeField] private GameObject shirtObj;
    [SerializeField] private GameObject backpackObj;

    private string[] colors;
    private string username;

    private void Update()
    {
        if (IsLocal)
            return;
        if(Camera.main != null)
            nameTag.transform.LookAt(Camera.main.transform);
        nameTag.text = list[Id].username;
    }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(ushort tick, Vector3 newPosition, Vector3 forward, bool isCrouching)
    {
        interpolator.NewUpdate(tick, newPosition);

        if (!IsLocal)
        {
            camTransform.forward = forward;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0));
            animationManager.SetCrouching(isCrouching);
            animationManager.AnimateBasedOnSpeed();
        } 
        else
        {
            if(camController == null)
                camController = camTransform.GetComponent<CameraController>();
            if (camController != null)
                camController.Crouch(isCrouching);
        }
    }

    private void SetPlayerColors(string[] colors)
    {
        for (int i = 0; i < baseObj.Length; i++)
        {
            ColorUtility.TryParseHtmlString(colors[0], out Color baseColor);
            foreach (GameObject obj in baseObj)
                obj.GetComponent<MeshRenderer>().material.color = baseColor;
        }

        ColorUtility.TryParseHtmlString(colors[1], out Color shirtColor);
        shirtObj.GetComponent<MeshRenderer>().material.color = shirtColor;

        ColorUtility.TryParseHtmlString(colors[1], out Color backpackColor);
        backpackObj.GetComponent<MeshRenderer>().material.color = backpackColor;
    }

    public static void Spawn(ushort id, string username, Vector3 position, string[] colors)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        } 
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
            player.SetPlayerColors(colors);
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;
        player.colors = colors;

        list.Add(id, player);
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3(), message.GetStrings());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUShort(), message.GetVector3(), message.GetVector3(), message.GetBool());
    }

    [MessageHandler((ushort)ServerToClientId.grabbedObject)]
    private static void GrabbedObject(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
            player.controller.UpdateGrabObject(message.GetInt());
    }
    #endregion
}
