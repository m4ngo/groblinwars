using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public enum GameStates { 
    PLAYING,
    GAMEOVER
}

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != null)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject[] NetworkPrefabs => networkPrefabs;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] networkPrefabs;

    [SerializeField] private Transform killFloorTransform;
    [SerializeField] private float killFloor = -3f;
    [SerializeField] private float killFloorStartDelay = 5f;
    [SerializeField] private float killFloorRaiseSpeed;
    [SerializeField] private float killFloorMaxHeight;

    [SerializeField] private float currentKillFloor;
    private float previousKillFloor;
    [SerializeField] private float startDelay = 0;
    [SerializeField] private int playersAlive;

    public GameStates gameState;
    private float gameoverTimer;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        startDelay = killFloorStartDelay;
        currentKillFloor = killFloor;
    }

    private void Update()
    {
        PlayerCounter();
        KillFloorHandler();

        if (previousKillFloor != currentKillFloor)
            SendLavalLevel();
        previousKillFloor = currentKillFloor;

        if (Player.list.Count > 1)
        {
            if (playersAlive <= 1 && gameState == GameStates.PLAYING)
            {
                foreach (Player player in Player.list.Values)
                {
                    if (player.TryGetComponent(out PlayerMovement move))
                    {
                        if(move.GetDead() <= 0)
                        {
                            SendWinner(player.Id);
                        }
                    }
                }
                gameState = GameStates.GAMEOVER;
            }
        }
        else
        {
            currentKillFloor = killFloor;
            startDelay = killFloorStartDelay;
        }

        if(gameState == GameStates.GAMEOVER)
        {
            gameoverTimer += Time.deltaTime;
            if(gameoverTimer >= 5)
            {
                currentKillFloor = killFloor;
                startDelay = killFloorStartDelay;

                foreach (Player player in Player.list.Values)
                {
                    if (player.TryGetComponent(out PlayerMovement move))
                    {
                        if (move.GetDead() > 10f)
                        {
                            move.SetDead(0.15f);
                        }
                        move.transform.parent = null;
                    }
                }
            }

            if(gameoverTimer >= 5.25f)
            {
                foreach (Player player in Player.list.Values)
                {
                    if (player.TryGetComponent(out PlayerMovement move))
                    {
                        move.transform.parent = null;
                        move.Respawn();
                    }
                }
                gameoverTimer = 0;
                gameState = GameStates.PLAYING;
            }
        }
    }

    void PlayerCounter()
    {
        int tempTally = Player.list.Count;
        foreach (Player player in Player.list.Values)
        {
            if (player.TryGetComponent(out PlayerMovement move))
            {
                if (move.GetDead() > 0)
                    tempTally--;
            }
        }
        playersAlive = tempTally;
    }

    void KillFloorHandler()
    {
        killFloorTransform.position = new Vector3(killFloorTransform.position.x, currentKillFloor - 3f, killFloorTransform.position.z);
        if (startDelay <= 0)
        {
            if (currentKillFloor > killFloorMaxHeight)
                currentKillFloor = killFloorMaxHeight;
            else if (currentKillFloor < killFloorMaxHeight)
                currentKillFloor += Time.deltaTime * killFloorRaiseSpeed;
        }
        else
            startDelay -= Time.deltaTime;
    }

    public float GetKillFloor()
    {
        return currentKillFloor;
    }

    private void SendWinner(ushort id)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.victory);
        message.AddUShort(id);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void SendLavalLevel()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.lavaLevel);
        message.AddFloat(currentKillFloor);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
