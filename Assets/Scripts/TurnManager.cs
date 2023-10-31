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
    private int TurnPlayerID;
    private Transform DiscardTF;
    private Transform LocalRackPrivateTF;

    private void Start()
    {
        GManager = Refs.GameManager.GetComponent<GameManager>();
        DiscardTF = Refs.Discard.transform;
        LocalRackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TurnPlayerID = GManager.Dealer;
    }

    public void StartGamePlay()
    {
        DiscardTF.gameObject.SetActive(true);
        if (IsMyTurn()) { EnableDiscard(); }
    }

    public void Discard(Transform tileTF)
    {
        // FIXME: Create an overload that takes tileID

        // Move the tile to the Discard area
        int tileID = tileTF.parent.GetComponent<Tile>().ID;

        if (GManager.Offline)
        {
            ShowTileInDiscard(tileTF);
            NextTurnHost(tileTF.parent.GetComponent<Tile>().ID);
        }

        else
        {
            RPC_ShowTileInDiscard(tileID);
            RPC_NextTurnHost(tileID);
            // FIXME: discard isn't showing on other machines during networked
            // FIXME: need to turn raycast off on discarded tiles
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShowTileInDiscard(int tileID)
    { ShowTileInDiscard(GameManager.TileList[tileID].transform.parent); }

    private void ShowTileInDiscard(Transform tileTF)
    {
        tileTF.GetComponentInChildren<TileLocomotion>().MoveTile(DiscardTF);
        tileTF.GetComponentInChildren<Image>().raycastTarget = false;
    }

    [Rpc (RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_NextTurnHost(int tileID)
    { NextTurnHost(tileID); }

    private void NextTurnHost(int tileID)
    {
        // TODO: add support for calling tiles

        // remove the tile from the player's rack list
        GManager.Racks[TurnPlayerID].Remove(GameManager.TileList[tileID]);
        // FIXME: getting an error here on the client when discarding

        TurnPlayerID = (TurnPlayerID + 1) % 4;  // increment turn
        
        // grab the next tile on the wall to show to the next player
        int nextTileID = GManager.Wall.Pop().GetComponent<Tile>().ID;

        // initiate turn on local machine or simulate AI turn
        if (GManager.Offline)   
        {
            if (IsMyTurn()) { NextTurnClient(nextTileID); }
            else { AITurn(nextTileID); }
        }
        else
        {
            PlayerRef turnPlayer = GManager.PlayerDict[TurnPlayerID];
            RPC_DisableDiscard();
            if (turnPlayer == PlayerRef.None) { AITurn(nextTileID); }
            else { RPC_NextTurnClient(turnPlayer, nextTileID); }
        }

        //FIXME: discard is showing 3 tiles sometimes when playing single player
        // on network, and the tiles sometimes rearrange??
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
        Tile tile = GameManager.TileList[tileID].GetComponent<Tile>();
        tile.MoveTile(LocalRackPrivateTF);
    }

    private void AITurn(int tileID)
    {
        // for now just discard whatever was picked up
        Discard(GameManager.TileList[tileID].transform.GetChild(0));
    }

    private void EnableDiscard()
    { DiscardTF.GetComponent<Image>().raycastTarget = true; }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_DisableDiscard() { DisableDiscard(); }

    private void DisableDiscard()
    { DiscardTF.GetComponent<Image>().raycastTarget = false; }

    private bool IsMyTurn() { return TurnPlayerID == GManager.LocalPlayerID; }
}
