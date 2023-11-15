using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class scratch : NetworkBehaviour, INetworkRunnerCallbacks
{

    public void OnEnable()
    {
        if (Runner != null)
        {
            Runner.AddCallbacks(this);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        CallInputStruct inputStruct = new();

        inputStruct.buttons.Set(Buttons.Space, Input.GetKey(KeyCode.Space));
        inputStruct.buttons.Set(Buttons.Return, Input.GetKey(KeyCode.Return));
        inputStruct.buttons.Set(Buttons.Left, Input.GetKey(KeyCode.LeftArrow));
        inputStruct.buttons.Set(Buttons.Right, Input.GetKey(KeyCode.RightArrow));

        input.Set(inputStruct);
    }

    public void OnDisable()
    {
        if (Runner != null)
        {
            Runner.RemoveCallbacks(this);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}