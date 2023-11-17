using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class NetworkCallbacks : MonoBehaviour
    , INetworkRunnerCallbacks
{
    // GAME OBJECTS
    [SerializeField]
    private ObjectReferences Refs;
    private NetworkRunner runner;
    private EventSystem ESystem;
    private GameObject DealMe;
    private GameObject Managers;
    public Transform ButtonsTF;


    private void Awake()
    {   // INITIALIZE GAME OBJECT FIELDS
        DealMe = Refs.DealMe;
        ButtonsTF = Refs.CallWaitButtons.transform;
        ESystem = Refs.EventSystem;
    }

    public async void StartGame(GameMode mode)
    {
        // CREATE RUNNER WITH INPUT
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        // START OR JOIN THE GAME
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Mah Jongg Room",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 4
        });
    }

    public void OnDisable()
    {
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    // INetworkRunnerCallbacks

    public void OnConnectedToServer(NetworkRunner runner)
    {   // logged on client with client joins (after OnPlayerJoined)
        Debug.Log("OnConnectedToServer");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        CallInputStruct inputStruct = new()
        {
            wait = (Input.GetKey(KeyCode.Space)
            && ButtonsTF.GetChild(0).gameObject.activeSelf)
            || ESystem.currentSelectedGameObject == ButtonsTF.GetChild(0).gameObject,

            pass = (Input.GetKey(KeyCode.Space)
            && ButtonsTF.GetChild(1).gameObject.activeSelf)
            || ESystem.currentSelectedGameObject == ButtonsTF.GetChild(1).gameObject,

            call = (Input.GetKey(KeyCode.Return)
            && ButtonsTF.GetChild(2).gameObject.activeSelf)
            || ESystem.currentSelectedGameObject == ButtonsTF.GetChild(2).gameObject
        };

        input.Set(inputStruct);
        ESystem.SetSelectedGameObject(null);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
 
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {   // logged on host when host joins
        // logged on host when client joins
        // logged on client when client joins
        
        Debug.Log("OnPlayerJoined");

        if (runner.IsServer)
        {
            if (player == runner.LocalPlayer)
            {
                Managers = runner.Spawn(Resources.Load<GameObject>("Prefabs/Managers")).gameObject;
                Managers.GetComponent<Setup>().H_Setup(player);
            }
            else
            {
                DealMe.GetComponent<DealClient>().Player = player;
                DealMe.SetActive(true);
            }
        }        
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
