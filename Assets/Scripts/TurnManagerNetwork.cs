using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class TurnManagerNetwork : NetworkBehaviour
{
    // everyone (local)
    public ObjectReferences Refs;
    private GameManager GManager;
    private IList<GameObject> TileList;
    private Transform DiscardTF;
    private Stack<GameObject> WallList;
    private Transform LocalRackTF;
    private NetworkRunner NRunner;

    // just for host
    private List<List<GameObject>> RackLists;
    public int TurnPlayerID { get; set; }

    private void Awake()
    {
        GManager = GetComponent<GameManager>();
        TileList = GameManager.TileList;
        DiscardTF = Refs.Discard.transform;
        WallList = GManager.Wall;
        LocalRackTF = Refs.LocalRack.transform;
        RackLists = GManager.Racks;
        TurnPlayerID = GManager.Dealer;
        NRunner = Refs.Runner.GetComponent<NetworkRunner>();
    }

    // Setup first turn
    public void StartGamePlay()
    {
        if (GManager.LocalPlayerID == GManager.Dealer)
        { DiscardTF.GetComponent<Image>().raycastTarget = true; }
    }

    // Client discards a tile
    public void C_Discard(Transform discardTileTF)
    { RPC_C2H_Discard(TileList.IndexOf(discardTileTF.gameObject)); }

    // RPC discard client to server
    [Rpc( RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer )]
    void RPC_C2H_Discard(int discardTileID, RpcInfo info = default)
    {
        int discardPlayerID = info.Source.PlayerId;
        H_NextTurn(discardTileID, discardPlayerID);
    }

    // Server does next turn logic
    void H_NextTurn(int discardTileID, int discardPlayerID)
    { 
        RPC_H2A_ShowDiscard(discardTileID);

        // update lists
        RackLists[discardPlayerID].Remove(TileList[discardTileID]);

        // TODO: wait for call

        // increment turn player
        TurnPlayerID = (TurnPlayerID + 1) % 4;

        // determine next tile
        int nextTileID = TileList.IndexOf(WallList.Pop());
        RPC_H2C_NextTurn(nextTileID);
    }

    // RPC discard server to all
    [Rpc( RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer )]
    void RPC_H2A_ShowDiscard(int discardTileID)
    { C_ShowDiscard(discardTileID); }

    // All clients show discarded tile
    void C_ShowDiscard(int discardTileID)
    { MoveTile(discardTileID, DiscardTF);  }

    // RPC sends next tile to client
    [Rpc( RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer )]
    void RPC_H2C_NextTurn(int nextTileID)
    { C_NextTurn(nextTileID); }

    // Client starts turn
    void C_NextTurn(int nextTileID)
    {
        MoveTile(nextTileID, LocalRackTF);  // display tile
        DiscardTF.GetComponent<Image>().raycastTarget = true; // enable discard
    }

    // Helper function to move tiles
    void MoveTile(int tileID, Transform destination)
    {
        TileList[tileID].transform
                        .GetChild(0)
                        .GetComponent<TileLocomotion>()
                        .MoveTile(destination);
    }
}
