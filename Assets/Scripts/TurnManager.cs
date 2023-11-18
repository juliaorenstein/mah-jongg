using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;

public class TurnManager : NetworkBehaviour
{
    public ObjectReferences Refs;
    private GameManager GManager;
    private Transform DiscardTF;
    private Transform LocalRackTF;
    private List<Transform> OtherRacksTFs;
    private TextMeshProUGUI TurnIndicatorText;
    private GameObject CallWaitButtons;
    private GameObject WaitButton;
    private GameObject PassButton;
    private NetworkObject NO;
    public string ExposeTileName;

    //private bool WaitingForPlayer = false;
    private TickTimer timer;

    private Dictionary<int,bool> PlayersWaiting;
    private bool AnyPlayerWaiting
    { get { return PlayersWaiting.Values.Any(val => val); } }

    private Dictionary<int, bool> PlayersPassing;
    private bool AnyPlayerPassing
    { get { return PlayersPassing.Values.Any(val => val); } }

    private Dictionary<int, bool> PlayersCalling;
    private bool AnyPlayerCalling
    { get { return PlayersCalling.Values.Any(val => val); } }

    private bool WaitingForCallers;

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
        UpdateCurrentPlayer();
        CallWaitButtons = Refs.CallWaitButtons;

        WaitButton = CallWaitButtons.transform.GetChild(0).gameObject;
        PassButton = CallWaitButtons.transform.GetChild(1).gameObject;

        PlayersWaiting = new();
        PlayersPassing = new();
        PlayersCalling = new();
    }

    // Setup first turn
    public void C_StartGamePlay()
    {
        foreach (int playerID in GManager.InputDict.Keys)
        {
            PlayersWaiting[playerID] = false;
            PlayersPassing[playerID] = false;
            PlayersCalling[playerID] = false;
        }

        DiscardTF.gameObject.SetActive(true);
        if (GManager.DealerID == GManager.LocalPlayerID)
        {
            DiscardTF.GetComponent<Image>().raycastTarget = true;
        }
        else if (GManager.PlayerDict[GManager.DealerID] == PlayerRef.None && Runner.IsServer)
        {
            H_AITurn(GManager.Racks[GManager.DealerID].Last().GetComponent<Tile>().ID);
        }
    }

    // FIXME: call wait buttons don't show on client and also the turn doesn't seem to increment when there's a client

    // Client discards a tile
    public void C_Discard(int discardTileID)
    {
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
            WaitingForCallers = true;
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
        if (Runner.LocalPlayer != GManager.PlayerDict[discardPlayerID])
        { CallWaitButtons.SetActive(true); }
    }


    //FIXME: wait isn't being respected in standalone

    public override void FixedUpdateNetwork()
    {
        // this if statement rules out time when we're not waiting for a tile to be called
        // but ALSO excludes clients because clients never have WaitingForCallers = true
        if (!WaitingForCallers) { return; }

        foreach ((int playerID, InputCollection playerInput) in GManager.InputDict)
        {
            PlayersWaiting[playerID] = playerInput.wait;
            PlayersPassing[playerID] = playerInput.pass;
            PlayersCalling[playerID] = playerInput.call;
        }

        if (timer.IsRunning)
        {
            if (AnyPlayerWaiting) { timer = TickTimer.None; }
            else if (AnyPlayerCalling) { Call(); }
            else if (timer.Expired(Runner)) { Pass(); }
        }
        else
        {
            if (AnyPlayerPassing) { Pass(); }
            else if (AnyPlayerCalling) { Call(); }
        }
    }

    // TODO: when calling, the tile should go to public rack
    // TODO: if called, other clients should see the tile go to otherracks

    // TODO: if a joker is discarded it can't be called
    // TODO: implement validation on calling

    void Call()
    {
        // going to make this so that only the first person who calls gets it.
        // TODO: add support for multiple people calling and giving to the next closest player turnwise
        foreach ((int playerID, bool calling) in PlayersCalling)
        {
            if (calling)
            {
                TurnPlayerID = playerID;
                break;
            }
        }
        H_CallTurn();
    }

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
        PlayerRef nextPlayer = InitializeNextTurn();
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
        PlayerRef callPlayer = InitializeNextTurn();
        GameObject callTile = DiscardTF.GetChild(DiscardTF.childCount - 1).gameObject;

        GManager.Racks[TurnPlayerID].Add(callTile); // TODO: track public tiles separately
        int callTileID = GameManager.TileList.IndexOf(callTile);
        // TODO: AI support for calling
        RPC_H2C_CallTurn(callPlayer, callTileID);
    }

    PlayerRef InitializeNextTurn()
    {
        WaitButton.SetActive(true);
        PassButton.SetActive(false);
        WaitingForCallers = false;
        //WaitingForPlayer = false;
        CallWaitButtons.SetActive(false);
        //timer = 0f;

        UpdateCurrentPlayer();
        return GManager.PlayerDict[TurnPlayerID];   // set next player
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
        MoveTile(callTileID, LocalRackTF.GetChild(0));

        // TODO: Add validation to make sure this is a legit hand? Or are players on their own
        ExposeTileName = GameManager.TileList[callTileID].name;
    }

    public void C_Expose(int exposeTileID)
    {
        MoveTile(exposeTileID, LocalRackTF.GetChild(0));
        RPC_C2A_Expose(exposeTileID);
        ExposeTileName = null;
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, InvokeLocal = false)]
    void RPC_C2A_Expose(int exposeTileID, RpcInfo info = default)
    {
        Transform exposePlayerRack = OtherRacksTFs[info.Source.PlayerId];
        Destroy(exposePlayerRack.GetChild(0).GetChild(0).gameObject);
        MoveTile(exposeTileID, exposePlayerRack.GetChild(1));
    }

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

    void UpdateCurrentPlayer()
    {
        TurnIndicatorText.SetText($"It's player {TurnPlayerID}'s turn.");
    }

    // TODO: this class is huuuuge
}
