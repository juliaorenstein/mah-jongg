using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    public ObjectReferences Refs;
    private GameManager GManager;
    private Transform DiscardTF;
    private Transform LocalRackTF;
    private Transform OtherRacksBoxTF;
    private TextMeshProUGUI TurnIndicatorText;
    private GameObject CallWaitButtons;
    private GameObject WaitButton;
    private GameObject PassButton;
    private NetworkObject NO;
    public string ExposeTileName;

    private TickTimer timer;

    private List<int> PlayersWaiting;
    private bool AnyPlayerWaiting
    { get { return PlayersWaiting.Count > 0; } }

    private List<int> PlayersCalling;
    private bool AnyPlayerCalling
    { get { return PlayersCalling.Count > 0; } }

    [Networked]
    public int TurnPlayerID { get; set; }

    public override void Spawned()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        // TODO: Don't use GameObject.Find
        GManager = GetComponent<GameManager>();
        DiscardTF = Refs.Discard.transform;
        LocalRackTF = Refs.LocalRack.transform;
        TurnIndicatorText = Refs.TurnIndicator
                                .transform
                                .GetChild(0)
                                .GetComponent<TextMeshProUGUI>();
        NO = GetComponent<NetworkObject>();
        TurnPlayerID = GManager.DealerID;
        CallWaitButtons = Refs.CallWaitButtons;
        OtherRacksBoxTF = Refs.OtherRacks.transform;

        WaitButton = CallWaitButtons.transform.GetChild(0).gameObject;
        PassButton = CallWaitButtons.transform.GetChild(1).gameObject;

        PlayersWaiting = new();
        PlayersCalling = new();
    }

    // Setup first turn
    public void C_StartGamePlay()
    {
        DiscardTF.gameObject.SetActive(true);
        if (GManager.DealerID == GManager.LocalPlayerID)
        {
            DiscardTF.GetComponent<Image>().raycastTarget = true;
        }
        else if (GManager.PlayerDict[GManager.DealerID] == PlayerRef.None && Runner.IsServer)
        {
            H_AITurn(GManager.Racks[GManager.DealerID].Last().GetComponent<TileComponent>().tile.ID);
        }
    }

    // Client discards a tile
    public void C_Discard(int discardTileID)
    {
        ExposeTileName = null;
        RPC_C2H_Discard(discardTileID);
        DiscardTF.GetComponent<Image>().raycastTarget = false;
    }

    // RPC discard client to server
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    void RPC_C2H_Discard(int discardTileID, RpcInfo info = default)
    {
        int discardPlayerID = info.Source.PlayerId;
        H_Discard(discardTileID, discardPlayerID);
    }

    // Server does next turn logic
    void H_Discard(int discardTileID, int discardPlayerID)
    {
        // update lists
        GManager.Racks[discardPlayerID].Remove(GameManager.TileList[discardTileID]);
        RPC_H2A_ShowDiscard(discardTileID);

        // wait for callers
        if (!Tile.IsJoker(discardTileID))
        {
            timer = TickTimer.CreateFromSeconds(Runner, 2f);
            RPC_H2A_ShowButtons(discardPlayerID);
        }
        else { StartCoroutine(WaitForJoker()); }
        //StartCoroutine(WaitForCallers());
    }

    // RPC discard server to all
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2A_ShowDiscard(int discardTileID)
    { C_ShowDiscard(discardTileID); }

    // All clients show discarded tile
    void C_ShowDiscard(int discardTileID)
    {
        MoveTile(discardTileID, DiscardTF);
        GameManager.TileList[discardTileID]
                   .GetComponentInChildren<Image>()
                   .raycastTarget = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2A_ShowButtons(int discardPlayerID)
    {
        C_ShowButtons(discardPlayerID);
    }

    void C_ShowButtons(int discardPlayerID)
    {
        if (Runner.LocalPlayer.PlayerId != discardPlayerID)
        { CallWaitButtons.SetActive(true); }

        // TODO: verify that this is working on clients now
    }

    public override void FixedUpdateNetwork()
    {
        // this check rules out times when players aren't calling
        // but also clients because they never have a timer set
        if (!timer.IsRunning) { return; }

        foreach ((int playerID, InputCollection playerInput) in GManager.InputDict)
        {
            if (playerInput.wait) { PlayersWaiting.Add(playerID); }
            if (playerInput.pass) { PlayersWaiting.Remove(playerID); }
            if (playerInput.call)
            {
                PlayersWaiting.Remove(playerID);
                PlayersCalling.Add(playerID);
            }
        }
        
        if (AnyPlayerWaiting) { return; }                           // if any player says wait, don't do anything
        else if (AnyPlayerCalling && timer.Expired(Runner))         // if any players call and timer is done/not running, do logic
        {
            int closestPlayerDelta = PlayersCalling.Select(
                playerID => playerID - TurnPlayerID).Min();
            TurnPlayerID = (TurnPlayerID + closestPlayerDelta + 4) % 4;

            Call();
        }
        else if (timer.Expired(Runner)) { Pass(); }                 // if nobody waited/called after 2s, pass
    }

    // TODO: when calling, the tile should go to public rack
    // TODO: if called, other clients should see the tile go to otherracks

    // TODO: if a joker is discarded it can't be called
    // TODO: implement validation on calling

    // FIXME: wait call buttons don't go away on clients for next turn

    void Call() { H_CallTurn(); } // silly

    void Pass()
    {
        TurnPlayerID = (TurnPlayerID + 1) % 4;
        H_NextTurn();
    }

    IEnumerator WaitForJoker()
    {
        yield return new WaitForSeconds(2);
        TurnPlayerID = (TurnPlayerID + 1) % 4;
        H_NextTurn();
    }

    void H_NextTurn()
    {
        PlayerRef nextPlayer = H_InitializeNextTurn();
        GameObject nextTile = GManager.Wall.Pop();
        nextTile.GetComponentInChildren<Image>().raycastTarget = true;
        
        GManager.Racks[TurnPlayerID].Add(nextTile);                 // add that tile to the player's rack list
        int nextTileID = GameManager.TileList.IndexOf(nextTile);    // find the ID of that tile
        if (nextPlayer == PlayerRef.None)                           // AI turn
        {
            H_AITurn(nextTileID);
            return;
        }
        RPC_H2C_NextTurn(nextPlayer, nextTileID);         // if it's a person, hand it over to that client
    }

    // FIXME: if two players wait and one passes, the game continues and the other still sees buttons
    // FIXME: rpc is acting very strange and inconsistent when called from build

    void H_CallTurn()
    {
        PlayerRef callPlayer = H_InitializeNextTurn();
        GameObject callTile = DiscardTF.GetChild(DiscardTF.childCount - 1).gameObject;

        GManager.Racks[TurnPlayerID].Add(callTile); // TODO: track public tiles separately
        int callTileID = GameManager.TileList.IndexOf(callTile);
        // TODO: AI support for calling
        RPC_H2C_CallTurn(callPlayer, callTileID);
    }

    PlayerRef H_InitializeNextTurn()
    {
        timer = TickTimer.None;
        PlayersWaiting.Clear(); // this shouldn't be needed
        PlayersCalling.Clear();
        RPC_H2A_ResetButtons();
        return GManager.PlayerDict[TurnPlayerID];   // set next player
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2A_ResetButtons() { C_ResetButtons(); }

    void C_ResetButtons()
    {
        TurnIndicatorText.SetText($"It's player {TurnPlayerID}'s turn.");
        WaitButton.SetActive(true);
        PassButton.SetActive(false);
        CallWaitButtons.SetActive(false);
    }

    // TODO: test network input, then implement into calling mechanism
    // TODO: add ability to navigate rack with arrow keys

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2C_CallTurn([RpcTarget] PlayerRef _, int callTileID)
    { C_CallTurn(callTileID); }

    void C_CallTurn(int callTileID)
    {
        C_Expose(callTileID);
        ExposeTileName = GameManager.TileList[callTileID].name;

        // FIXME: if a player puts an exposed tile back on their rack, remove it from screen
    }

    // TODO: during expose, add a never mind button that moves onto the next caller or passes

    public void C_Expose(int exposeTileID)
    {
        MoveTile(exposeTileID, LocalRackTF.GetChild(0));
        RPC_C2A_Expose(exposeTileID);
        if (ReadyToContinue()) { DiscardTF.GetComponent<Image>().raycastTarget = true; }
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, InvokeLocal = false)]
    void RPC_C2A_Expose(int exposeTileID, RpcInfo info = default)
    { C_OtherPlayerExposes(exposeTileID, info.Source.PlayerId); }

    void C_OtherPlayerExposes(int exposeTileID, int playerID)
    {
        int rackID = (playerID - Runner.LocalPlayer.PlayerId + 4) % 4 - 1;
        Transform exposePlayerRack = OtherRacksBoxTF.GetChild(rackID);
        Destroy(exposePlayerRack.GetChild(1).GetChild(0).gameObject);
        MoveTile(exposeTileID, exposePlayerRack.GetChild(0));

        // FIXME: not working
    }

    bool ReadyToContinue() { return true; } // TODO: later make sure it's >2 and valid group

    void H_AITurn(int newTileID)
    {
        int discardTileID = newTileID; // for now just discard what was picked up
        H_Discard(discardTileID, TurnPlayerID);
    }

    // Helper function to move tiles
    void MoveTile(int tileID, Transform destination)
    {
        GameManager.TileList[tileID].transform
                   .GetComponentInChildren<TileLocomotion>()
                   .MoveTile(destination);
    }

    // TODO: this class is huuuuge
    // FIXME: when client calls host doesn't see the first exposed tile
    // FIXME: if client exposes the same tile multiple times, more tiles get destroyed on the other clients
}
