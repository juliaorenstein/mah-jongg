using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class CharlestonManager : NetworkBehaviour
{
    public ObjectReferences Refs;
    public Transform CharlestonTF;
    public Transform CharlestonBoxTF;
    private Button PassButton;
    private CharlestonPassButton PassButtonScript;
    private GameManager GManager;
    private Transform RackPrivateTF;
    private Transform TilePoolTF;
    //private int[][] PassArr = new int[4][];
    private List<List<int>> PassArr = new() { new(), new(), new(), new() };
    private List<List<int>> RecArr = new() { new(), new(), new(), new() };
    private readonly List<int> StealList = new List<int>() { 2, 5 };
    private readonly int OptPass = 6;

    [Networked] private bool Opt { get; set; }
    [Networked] public int Counter { get; set; }

    public string Direction()
    {
        return Direction(Counter);
    }

    public string Direction(int counter)
    {
        return counter switch
        {
            // first right
            0 or 5 => "Right",
            // first over
            1 or 4 or 6 => "Over",
            // first left
            2 or 3 => "Left",
            _ => "Done",
        };
    }

    // host
    private int PlayersReady = 0;

    public override void Spawned()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        // FIXME: find call
        CharlestonTF = Refs.Charleston;
        CharlestonBoxTF = CharlestonTF.GetChild(0);
        PassButton = CharlestonTF.GetComponentInChildren<Button>();
        PassButtonScript = PassButton.GetComponent<CharlestonPassButton>();
        GManager = Refs.Managers.GetComponent<GameManager>();
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TilePoolTF = Refs.TilePool.transform;
        Opt = false;
    }

    // FIXME: first, second, and fifth (??) passes are not smooth
    // FIXME: make passes come from the right direction

    public void C_CheckDone()
    {
        bool ready = true;
        bool jokers = false;

        foreach (Transform chSpot in CharlestonBoxTF)
        {
            // check if the spot is populated
            if (chSpot.childCount == 0)
            {
                if (!Opt) { ready = false; } // if it's an optional pass, we are ready whenev
                continue;
            }

            // if populated, check that there are no jokers
            if (chSpot.GetChild(0).name == "Joker")
            {
                jokers = true;
                ready = false;
                PassButtonScript.NoJokers();
                break;
            }
        }

        if (!jokers) { PassButtonScript.UpdateButton(); }
        if (ready) { PassButton.interactable = true; }
        else { PassButton.interactable = false; }
    }

    // client presses the button to start the pass, and the tiles in the
    // Charleston box get sent to the host
    public void C_StartPass()
    {
        Transform tileTF;
        Transform chSpotTF;

        // collect the tiles to pass from the Charleston box
        // and move the tiles off screen
        List<int> tileIDsToPass = new();
        for (int i = 0; i < 3; i++)
        {
            chSpotTF = CharlestonBoxTF.GetChild(i);
            if (chSpotTF.childCount > 0)
            {
                tileTF = chSpotTF.GetChild(0);
                tileTF.GetChild(0).GetComponent<TileLocomotion>().MoveTile(TilePoolTF);
                tileIDsToPass.Add(tileTF.GetComponent<Tile>().ID);
            }
        }

        // give the tiles to the host
        RPC_C2H_StartPass(tileIDsToPass.ToArray());
    }

    // send tiles from Client to Host
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    void RPC_C2H_StartPass(int[] tileIDsToPass, RpcInfo info = default)
    {
        H_StartPass(info.Source.PlayerId, tileIDsToPass);
    }

    // host checks if all players have initiated the pass and does the pass if so
    void H_StartPass(int sourcePlayerId, int[] tileIDsToPass)
    {
        // if there are AI players, figure out their passes when the first player is ready
        if (PlayersReady == 0)
        {
            for (int playerID = 0; playerID < 4; playerID++)
            {
                if (GManager.PlayerDict[playerID] == PlayerRef.None)
                { H_AICharlestonPass(playerID); }
            }
        }

        // update pass array
        PassArr[sourcePlayerId] = tileIDsToPass.ToList();
        PlayersReady += 1;
        if (PlayersReady == 4) { H_Pass(); }
    }

    // AI passes last three tiles every time
    void H_AICharlestonPass(int playerID)
    {
        List<GameObject> aiRack = GManager.Racks[playerID];
        for (int i = 0; i < 3; i++)
        {
            PassArr[playerID] = aiRack.GetRange(aiRack.Count - 3, 3)
                                      .Select(tile => tile.GetComponent<Tile>().ID)
                                      .ToList();
        }
        PlayersReady++;
    }

    // host does all the pass logic and reveals new tiles to players
    void H_Pass()
    {
        if (Counter != OptPass) { PassLogic(); }
        else { OptPassLogic(); }

        // TODO: update racklist management to refer to ints, not gameobjects

        // now update racklists again and send to clients
        for (int targetID = 0; targetID < 4; targetID++)
        {
            RecArr[targetID].ForEach(tileID =>
                GManager.Racks[targetID].Add(GameManager.TileList[tileID]));

            if (GManager.PlayerDict[targetID] != PlayerRef.None) // prevent host from receiving all the AI tiles
            {
                RPC_H2C_SendTiles(GManager.PlayerDict[targetID], RecArr[targetID].ToArray());
            }

            foreach (List<int> list in RecArr) { list.Clear(); }
        }

        // prep for next pass
        PlayersReady = 0;
        Counter++;
        if (StealList.Contains(Counter)) { Opt = true; }
        else { Opt = false; }
    }

    void PassLogic()
    {
        int targetID;
        List<int> WaitingForTilesList = PassArr.Select(list => list.Count).ToList();
        // first remove things from racklists (and set up RecArr while we're at it)
        for (int sourceID = 0; sourceID < 4; sourceID++)
        {
            PassArr[sourceID].ForEach(tileID =>
                GManager.Racks[sourceID].Remove(GameManager.TileList[tileID]));
        }

        // now rearrange everything
        while (PassArr.Any(subArr => subArr.Any()))
        {
            for (int sourceID = 0; sourceID < 4; sourceID++)
            {
                targetID = PassTargetID(sourceID, Direction());
                foreach (int tileID in PassArr[sourceID])
                {
                    if (RecArr[targetID].Count < WaitingForTilesList[targetID])
                    {
                        RecArr[targetID].Add(tileID);
                    }
                    else { PassArr[targetID].Add(tileID); }
                }
                PassArr[sourceID].Clear();
            }
        }
    }

    void OptPassLogic()
    {
        for (int i = 0; i < 2; i++)
        {
            int countA = PassArr[i].Count;
            int countB = PassArr[i + 2].Count;

            if (countA != countB)
            {
                RecArr[i] = PassArr[i + 2];
                RecArr[i + 2] = PassArr[i];
            }
            else
            {
                // TODO: implement functionality for when optional pass is not the same number
            }
        }
    }

    // helper function to determine the target of the pass
    int PassTargetID(int sourceID, string direction)
    {
        // calculate the target off the pass rpc based on the local
        // player id and the charleston counter
        int shift;

        switch (direction)
        {
            case "Right":
                shift = 3;
                break;
            case "Over":
                shift = 2;
                break;
            default:
                shift = 1;
                break;
        }
        return (sourceID + shift) % 4;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2C_SendTiles([RpcTarget] PlayerRef _, int[] tileIDsToSend)
    {
        { C_ReceiveTiles(tileIDsToSend); }
    }

    void C_ReceiveTiles(int[] tileIDsToReceive)
    {
        foreach (int tileID in tileIDsToReceive)
        { TileLocomotion.MoveTile(tileID, RackPrivateTF); }

        PassButton.GetComponent<CharlestonPassButton>().UpdateButton(Counter + 1);
    }
}
