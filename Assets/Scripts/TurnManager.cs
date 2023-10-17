using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public ObjectReferences Refs;
    private GameManager GManager;
    [Networked]
    public int TurnPlayerID { get; set; }
    private Transform DiscardTF;
    private Transform LocalRackPrivateTF;

    private void Awake()
    {
        GManager = Refs.GameManager.GetComponent<GameManager>();
        DiscardTF = Refs.Discard.transform;
        LocalRackPrivateTF = Refs.LocalRack.transform.GetChild(1);
    }

    private void Start()
    {
        TurnPlayerID = GManager.Dealer;
    }

    public void FirstTurn()
    {
        DiscardTF.gameObject.SetActive(true);
        if (IsMyTurn()) { EnableDiscard(); }
    }

    public void Discard(Transform tileTF)
    {
        // Move the tile to the Discard area
        int tileID = tileTF.parent.GetComponent<Tile>().ID;
        // FIXME: the discarded tile is only showing for the player that discarded it
        // FIXME: AIs aren't discarding the tile image
        if (GManager.Offline)
        {
            ShowTileInDiscard(tileTF);
            NextTurnHost(tileTF.parent.GetComponent<Tile>().ID);
        }
        else
        {
            RPC_ShowTileInDiscard(tileTF);
            RPC_NextTurnHost(tileID);
        }
    }

    [Rpc (RpcSources.All, RpcTargets.All)]
    public void RPC_ShowTileInDiscard(Transform tileTF)
    { ShowTileInDiscard(tileTF); }

    private void ShowTileInDiscard(Transform tileTF)
    { tileTF.GetComponent<TileLocomotion>().MoveTile(DiscardTF); }

    [Rpc (RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_NextTurnHost(int tileID, RpcInfo info = default)
    { NextTurnHost(tileID); }

    private void NextTurnHost(int tileID)
    {
        // TODO: TurnPlayerID and playerID might be redundant
        // TODO: add support for calling tiles

        // remove the tile from the player's rack list
        GManager.Racks[TurnPlayerID].Remove(GManager.TileList[tileID]);
        TurnPlayerID = (TurnPlayerID + 1) % 4;  // increment turn
        
        // grab the next tile on the wall to show to the next player
        int nextTileID = GManager.Wall.Pop().GetComponent<Tile>().ID;

        // nitiate turn on local machine or simulate AI turn
        if (GManager.Offline)   
        {
            if (IsMyTurn()) { NextTurnClient(nextTileID); }
            else { AITurn(nextTileID); }
        }
        else
        {
            PlayerRef turnPlayer = GManager.PlayerDict[TurnPlayerID];
            if (turnPlayer == PlayerRef.None) { AITurn(tileID); }
            RPC_DisableDiscard();
            RPC_NextTurnClient(turnPlayer, nextTileID);
        } 
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NextTurnClient([RpcTarget] PlayerRef _, int tileID)
    { NextTurnClient(tileID); }

    private void NextTurnClient(int tileID)
    {
        EnableDiscard();
        ShowNextTile(tileID);
    }    

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowNextTile([RpcTarget] PlayerRef _, int nextTileID)
    { ShowNextTile(nextTileID); }

    private void ShowNextTile(int tileID)
    {
        Tile tile = GManager.TileList[tileID].GetComponent<Tile>();
        tile.MoveTile(LocalRackPrivateTF);
    }

    private void AITurn(int tileID)
    {
        // for now just discard whatever was picked up
        Discard(GManager.TileList[tileID].transform.GetChild(0));
        // FIXME: ugly line of code
    }

    private void EnableDiscard()
    { DiscardTF.GetComponent<Image>().raycastTarget = true; }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_DisableDiscard() { DisableDiscard(); }

    private void DisableDiscard()
    { DiscardTF.GetComponent<Image>().raycastTarget = false; }

    private bool IsMyTurn() { return TurnPlayerID == GManager.LocalPlayerID; }
}
