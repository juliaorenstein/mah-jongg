using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class CharlestonManager : NetworkBehaviour
{

    // everyone
    public ObjectReferences Refs;
    public Transform CharlestonTF;
    public Transform CharlestonBoxTF;
    private Button PassButton;
    private CharlestonPassButton PassButtonScript;
    private GameManager GManager;
    private Transform RackPrivateTF;
    private Transform TilePoolTF;
    private int[][] PassArr = new int[4][];

    [Networked] public int Counter { get; set; }

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
    }

    // FIXME: first, second, and fifth (??) passes are not smooth
    // FIXME: make passes come from the right direction

    public void C_CheckDone()
    {
        bool ready = true;
        bool jokers = false;

        foreach (Transform chSpot in CharlestonBoxTF)
        {
            // check all spots are populated
            if (chSpot.childCount == 0)
            {
                ready = false;
                // don't break because we still want to check for jokers
            }

            // check that there are no jokers
            else if (chSpot.GetChild(0).name == "Joker")
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

        // collect the tiles to pass from the Charleston box
        // and move the tiles off screen
        int[] tileIDsToPass = new int[3];
        for (int i = 0; i < 3; i++)
        {
            tileTF = CharlestonBoxTF.GetChild(i).GetChild(0);
            tileTF.GetChild(0).GetComponent<TileLocomotion>().MoveTile(TilePoolTF);
            tileIDsToPass[i] = tileTF.GetComponent<Tile>().ID;
        }

        // give the tiles to the host
        //if (GManager.Offline) { H_StartPass(3, tileIDsToPass); }
        //else { RPC_C2H_StartPass(tileIDsToPass); }
        RPC_C2H_StartPass(tileIDsToPass);
        // prep for next pass

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
        PassArr[sourcePlayerId] = tileIDsToPass;
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
                                      .ToArray();
        }
        PlayersReady++;
    }

    // host does all the pass logic and reveals new tiles to players
    void H_Pass()
    {
        int targetID;
        PlayerRef targetPlayerRef;
        int[] tileIDsToSend;
        Counter++;

        for (int sourceID = 0; sourceID < 4; sourceID++)
        {
            targetID = PassTargetID(sourceID, Direction());
            targetPlayerRef = GManager.PlayerDict[targetID];
            tileIDsToSend = PassArr[sourceID];

            // update the lists
            foreach (int tileID in tileIDsToSend)
            {
                // remove from source
                GManager.Racks[sourceID].Remove(GameManager.TileList[tileID]);
                // add to target
                GManager.Racks[targetID].Add(GameManager.TileList[tileID]);
            }

            // send to client
            if (targetPlayerRef != PlayerRef.None) // prevent host from receiving all the AI tiles
            {
                RPC_H2C_SendTiles(targetPlayerRef, tileIDsToSend);
            }
        }

        // prep for next pass
        Array.Clear(PassArr, 0, PassArr.Length);
        PlayersReady = 0;
    }

    // helper function to determine what direction to pass
    public string Direction()
    {
        switch (Counter)
        {
            case 0:         // first right
            case 5:         // second right
                return "Right";
            case 1:         // first across
            case 4:         // second across
            case 6:         // optional
                return "Across";
            case 2:         // first left
            case 3:         // second left
                return "Left";
            default:
                return "Done";
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
                shift = 1;
                break;
            case "Across":
                shift = 2;
                break;
            default:
                shift = 3;
                break;
        }
        return (sourceID + shift) % 4;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_H2C_SendTiles([RpcTarget] PlayerRef player, int[] tileIDsToSend)
    {
        { C_ReceiveTiles(tileIDsToSend); }
    }

    void C_ReceiveTiles(int[] tileIDsToReceive)
    {
        foreach (int tileID in tileIDsToReceive)
        { TileLocomotion.MoveTile(tileID, RackPrivateTF); }

        PassButton.GetComponent<CharlestonPassButton>().UpdateButton();
    }
}
