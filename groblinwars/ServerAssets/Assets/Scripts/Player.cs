using Riptide;
using Riptide.Utils;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public PlayerMovement Movement => movement;
    public PlayerCombat Combat => combat;

    public string[] colors { get; private set; } //OK THIS ONE IS DUMB AND I SHOULD REALLY JUST USE 3 COLORS BUT WHATEVER.
    //INDEX 0: base color
    //INDEX 1: shirt color
    //INDEX 2: backpack color
    public int hat;

    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private Transform[] spawnpoints;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username, string[] colors, int hat)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        foreach (NetworkObject networkObject in NetworkObject.list.Values)
            networkObject.SendSpawned(id);


        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, NetworkManager.Singleton.GetSpawnpoint(), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        player.colors = colors;
        player.hat = hat;

        player.SendSpawned();
        list.Add(id, player);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        message.AddStrings(colors);
        message.AddInt(hat);
        return message;
    }
    
    //PROCEEDING SECTION HANDLES ALL OF THE PLAYERS CLIENT TO SERVER MESSAGES

    //this is essentially when the player spawns in
    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString(), message.GetStrings(), message.GetInt());
    }

    [MessageHandler((ushort)ClientToServerId.movementInput)]
    private static void MovementInput(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.movement.SetInput(message.GetBools(10), message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.interactiveInput)]
    private static void InteractiveInput(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.combat.InputReceived(message.GetInt(), message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.leftClick)]
    private static void LeftClick(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.combat.LeftClick(message.GetFloat());
    }

    [MessageHandler((ushort)ClientToServerId.rightClick)]
    private static void RightClick(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.combat.RightClick(message.GetUShort());
    }

    [MessageHandler((ushort)ClientToServerId.hammer)]
    private static void Hammer(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.combat.Hammer(message.GetUShort());
    }
    #endregion
}
