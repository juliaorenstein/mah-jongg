using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    // GAME OBJECTS
    [SerializeField]
    private ObjectReferences Refs;
    private NetworkRunner runner;
    private Setup Setup;

    private void Awake()
    {   // INITIALIZE GAME OBJECT FIELDS
        Setup = Refs.GameManager.GetComponent<Setup>();
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

    // INETWORKRUNNERCALLBACKS IMPLEMENTATION

    public void OnConnectedToServer(NetworkRunner runner)
    {   // LOGGED ON CLIENT WHEN CLIENT JOINS (after OnPlayerJoined)
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
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
 
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {   // LOGGED ON HOST WHEN HOST JOINS
        // LOGGED ON HOST WHEN CLIENT JOINS
        // LOGGED ON CLIENT WHEN CLIENT JOINS
        
        Debug.Log("OnPlayerJoined");
        Setup.SetupGame(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
