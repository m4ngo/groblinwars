using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

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

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject[] NetworkPrefabs => networkPrefabs;
    public GameObject[] Hats => hats;

    [Header("Prefab")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] networkPrefabs;
    [SerializeField] private GameObject[] hats;

    private void Awake()
    {
        Singleton = this;
    }

    [MessageHandler((ushort)ServerToClientId.lavaLevel)]
    private static void LavaLevel(Message message)
    {
        GameObject.FindWithTag("Lava").transform.position = new Vector3(0, message.GetFloat(), 0);
    }
}
