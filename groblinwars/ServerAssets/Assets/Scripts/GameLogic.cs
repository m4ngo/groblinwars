using System.Collections;
using System.Linq;
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

    [Header("Game Logic")]

    [SerializeField] private Transform killFloorTransform;
    [SerializeField] private float killFloor = -3f;
    [SerializeField] private float killFloorStartDelay = 5f;
    [SerializeField] private float killFloorRaiseSpeed;
    [SerializeField] private float killFloorMaxHeight;

    [SerializeField] private float currentKillFloor;
    private float previousKillFloor;
    [SerializeField] private float startDelay = 0;
    [SerializeField] private int playersAlive;

    [Header("Spawn Objects")]
    [SerializeField] private GameObject timedSpawner;

    [SerializeField] private float timeBtwObjectSpawns;
    [SerializeField] private int minSpawns, maxSpawns;
    [SerializeField] private int[] networkObjectsThatCanSpawn;
    private float objectSpawnTimer;

    [SerializeField] private Transform[] objectSpawnpoints;
    [SerializeField] private List<Transform> spawns = new List<Transform>();

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
        spawns = objectSpawnpoints.ToList();
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
            if(playersAlive > 1 && gameState == GameStates.PLAYING)
            {
                objectSpawnTimer -= Time.deltaTime;
                if(objectSpawnTimer <= 0)
                {
                    objectSpawnTimer = timeBtwObjectSpawns;
                    int rand = Random.Range(minSpawns, maxSpawns + 1);
                    for (int i = 0; i < rand; i++)
                    {
                        int randObj = Random.Range(0, networkObjectsThatCanSpawn.Length);

                        for (int j = spawns.Count - 1; j >= 0; j--)
                        {
                            if (spawns[j].transform.position.y <= currentKillFloor)
                                spawns.RemoveAt(j);
                        }

                        int randSpawn = Random.Range(0, spawns.Count);

                        Instantiate(timedSpawner, spawns[randSpawn].position, Quaternion.identity).GetComponent<TimedSpawner>().prefabIndex = networkObjectsThatCanSpawn[randObj];
                        SendSpawnObject(spawns[randSpawn].position);
                    }
                }
            }

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
                    if (player.TryGetComponent(out PlayerCombat combat))
                    {
                        combat.LeftClick(0.25f);
                    }
                }

                //destroy all the objects
                foreach (NetworkObject obj in NetworkObject.list.Values)
                {
                    if (obj.gameObject.layer == 7)
                        obj.DestroyObject();
                }
                objectSpawnTimer = timeBtwObjectSpawns;
                spawns = objectSpawnpoints.ToList();
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

                //destroy all the objects
                foreach (NetworkObject obj in NetworkObject.list.Values)
                {
                    if (obj.gameObject.layer == 7)
                        obj.DestroyObject();
                }
                objectSpawnTimer = timeBtwObjectSpawns;
                spawns = objectSpawnpoints.ToList();
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

    private void SendSpawnObject(Vector3 position)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.spawnObject);
        message.Add(position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
