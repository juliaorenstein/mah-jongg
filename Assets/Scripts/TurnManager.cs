using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TurnManager : NetworkBehaviour
    ,ISpawned
    ,IAfterSpawned
{
    // everyone (local)
    public ObjectReferences Refs;
    private GameManager GManager;
    private Transform DiscardTF;
    private Transform LocalRackTF;
    private TextMeshProUGUI TurnIndicatorText;

    // just for host
    private List<List<GameObject>> RackLists;

    [Networked]
    public int TurnPlayerID { get; set; }

    // FIXME: with 4 sessions open, discarding is synced :) But all players get
    // the new tile and host is behind one turn on the display

    public override void Spawned()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        // FIXME: Find call
        GManager = GetComponent<GameManager>();
        DiscardTF = Refs.Discard.transform;
        LocalRackTF = Refs.LocalRack.transform;
        RackLists = GManager.Racks;
        TurnIndicatorText = Refs.TurnIndicator
                                .transform
                                .GetChild(0)
                                .GetComponent<TextMeshProUGUI>();
    }

    public void AfterSpawned()
    {
        TurnPlayerID = GManager.Dealer;
    }

    // Setup first turn
    public void StartGamePlay()
    {
        DiscardTF.gameObject.SetActive(true);
        UpdateTurnIndicator();
        if (GManager.LocalPlayerID == GManager.Dealer)
        {
            DiscardTF.GetComponent<Image>().raycastTarget = true;
        }
    }

    // Client discards a tile
    public void C_Discard(Transform discardTileTF)
    {
        RPC_C2H_Discard(discardTileTF.GetComponent<Tile>().ID);
        DiscardTF.GetComponent<Image>().raycastTarget = false;
    }

    // RPC discard client to server
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    void RPC_C2H_Discard(int discardTileID, RpcInfo info = default)
    {
        int discardPlayerID = info.Source.PlayerId;
        H_NextTurn(discardTileID, discardPlayerID);
    }

    // Server does next turn logic
    void H_NextTurn(int discardTileID, int discardPlayerID)
    {
        // update lists
        RackLists[discardPlayerID].Remove(GameManager.TileList[discardTileID]);

        // increment turn player
        UpdateTurnPlayerID();
        RPC_H2A_ShowDiscard(discardTileID);

        // TODO: wait for call

        PlayerRef nextPlayer = GManager.PlayerDict[TurnPlayerID];   // set next player
        GameObject nextTile = GManager.Wall.Pop();                  // set next tile from wall
        GManager.Racks[TurnPlayerID].Add(nextTile);                 // add that tile to the player's rack list
        int nextTileID = GameManager.TileList.IndexOf(nextTile);    // find the ID of that til
        if (nextPlayer == PlayerRef.None) { H_AITurn(nextTileID); }   // if it's AI, do that turn
        RPC_H2C_NextTurn(nextPlayer, nextTileID);         // if it's a person, hand it over to that client
        
    }
    // FIXME: with >1 player, the discard is showing up fine for clients but showing up in the rack
    // for the host

    // RPC discard server to all
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2A_ShowDiscard(int discardTileID)
    { C_ShowDiscard(discardTileID); }

    // All clients show discarded tile
    void C_ShowDiscard(int discardTileID)
    {
        MoveTile(discardTileID, DiscardTF);
        UpdateTurnIndicator();
    }

    // RPC sends next tile to client
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2C_NextTurn([RpcTarget] PlayerRef _, int nextTileID)
    { C_NextTurn(nextTileID); }

    // Client starts turn
    void C_NextTurn(int nextTileID)
    {
        MoveTile(nextTileID, LocalRackTF.GetChild(1));  // display tile
        DiscardTF.GetComponent<Image>().raycastTarget = true; // enable discard
    }

    void H_AITurn(int newTileID)
    {
        int discardTileID = newTileID; // for now just discard what was picked up
        H_NextTurn(discardTileID, TurnPlayerID);
    }

    // Helper function to move tiles
    void MoveTile(int tileID, Transform destination)
    {
        GameManager.TileList[tileID].transform
                        .GetChild(0)
                        .GetComponent<TileLocomotion>()
                        .MoveTile(destination);
    }

    void UpdateTurnPlayerID()
    {
        TurnPlayerID = (TurnPlayerID + 1) % 4;
    }

    void UpdateTurnIndicator()
    {
        TurnIndicatorText.SetText($"It's player {TurnPlayerID}'s turn.");
    }
}
