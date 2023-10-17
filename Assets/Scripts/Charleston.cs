using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.Types;

public class Charleston : NetworkBehaviour
{
    public ObjectReferences Refs;
    private Button PassButton;
    private CharlestonPassButton PassButtonScript;
    private GameManager GManager;
    private NetworkRunner runner;
    private Transform RackPrivateTF;
    private Transform TilePoolTF;
    private int[] TilesToPass = new int[3];
    private int[][] PassArr = new int[4][];
    private int Counter = 0;
    public string Direction = "Right";
    private int PlayersReady = 0;

    private void Awake()
    {
        PassButton = Refs.CharlestonPassButton.GetComponent<Button>();
        PassButtonScript = PassButton.GetComponent<CharlestonPassButton>();
        GManager = Refs.GameManager.GetComponent<GameManager>();
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TilePoolTF = Refs.TilePool.transform;
    }

    private void Start()
    {   // if playing online, everybody except local player is ready to pass
        // don't think i actually need this
        // if (GManager.Offline) { PlayersReady = 3; }
    }

    public void CheckDone()
    {   // check if the user has placed three tiles in the Charleston box
        // and is eligible to click the pass button.

        bool ready = true;
        foreach (Transform chSpot in transform)
        {
            if (chSpot.childCount == 0)
            {
                ready = false;
                break;
            }
        }

        if (ready) { PassButton.interactable = true; }
        else { PassButton.interactable = false; }
    }
    
    public void StartPassFromLocal()
    {   // on local machine, put pass tiles back into the TilePool
        // kick off the rest of the pass via RPC

        // TODO: account for optional charlestons

        // first set up runner
        if (Counter == 0 && !GManager.Offline)
        {
            runner = Refs.Runner.GetComponent<NetworkRunner>();
        }

        UpdateDirection();
        for (int i = 0; i < 3; i++)     
        {   
            Tile tile = transform.GetChild(i).GetChild(0).GetComponent<Tile>();
            TilesToPass[i] = tile.ID;   // set array of tiles to pass
            // move tile to TilePool for local player
            tile.MoveTile(TilePoolTF);  
        }

        // kick off when host is ready on the first pass. After that, do it at
        // the end of each Charleston.
        if (Counter == 0 && (GManager.Offline || runner.IsServer)) { AIMechanics(); }

        // if offline, skip to the actual pass
        if (GManager.Offline) { HostPassLogic(); }
        // if networked send player ready to host
        else { RPC_PlayerReady(TilesToPass); }

    }

    // from player to host
    [Rpc (RpcSources.All
        , RpcTargets.StateAuthority
        , TickAligned = false
        , HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PlayerReady(int[] tiles, RpcInfo rpcInfo = default)
    {   // should be executed on server to populate TileArr and kick off
        // pass rpc when everybody is ready
        PlayersReady++;
        PassArr[rpcInfo.Source.PlayerId] = tiles;

        // if all players are ready, kick off the actual pass
        if ( PlayersReady == 4 ) { HostPassLogic(); }
    }

    void HostPassLogic()
    {   // move tiles on the racklists, then switch around the gameobjects to match
        // TODO: blind pass logic
        // TODO: optional pass logic
        // TODO: disallow passing jokers
        // TODO: double-click functionality
        // FIXME: things are not working
        foreach (List<GameObject> rack in GManager.Racks)
        {
            string rackStr = $"rack {GManager.Racks.IndexOf(rack)}: ";
            foreach(GameObject tileGO in rack)
            {
                rackStr += tileGO.name + ", ";
            }
            Debug.Log(rackStr);
        }
        
        for (int sourceID = 0; sourceID < 4; sourceID++)
        {   // update lists
            int targetID = PassTargetID(sourceID);
            UpdateRackLists(sourceID, targetID, PassArr[sourceID]);

            // if online, update rack gameobject for each player
            if (!GManager.Offline)
            {
                PlayerRef targetRef = GManager.PlayerDict[targetID];
                RPC_ReceiveTiles(targetRef, PassArr[sourceID]);
            }

            // if offline, update rack gameobject when targetID=3
            if (targetID == 3 && GManager.Offline)
            {
                ReceiveTiles(PassArr[sourceID]);
            }
        }

        Counter++;                          // increase charleston counter
        if (Counter == 7) { Counter = -1; }
        // TODO: expand logic to set Counter to -1 when players quit after first
        // three passes or don't do optional.
        UpdateDirection();
        PassButtonScript.UpdateButton(Counter);    // update button text
        AIMechanics();                      // start the next pass on the AIs.
    }

    void UpdateRackLists(int sourceID, int targetID, int[] tileIDs)
    {
        List<GameObject> sourceRack = GManager.Racks[sourceID];
        List<GameObject> targetRack = GManager.Racks[targetID];
        GameObject tileGO;

        foreach (int tileID in tileIDs)
        {
            tileGO = GManager.TileList[tileID];
            sourceRack.Remove(tileGO);
            targetRack.Add(tileGO);
        }
    }

    // from host to target player
    [Rpc (RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_ReceiveTiles([RpcTarget] PlayerRef _, int[] tiles)
    {
        ReceiveTiles(tiles);
    }

    public void ReceiveTiles(int[] tiles)
    {
        foreach (int ID in tiles)
        {
            GManager.TileList[ID]
                    .GetComponentInChildren<TileLocomotion>()
                    .MoveTile(RackPrivateTF);
        }
    }

    int PassTargetID(int sourceID)
    {
        // calculate the target off the pass rpc based on the local
        // player id and the charleston counter
        int shift;

        switch (Direction)
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

    void UpdateDirection()
    {
        switch (Counter)
        {
            case 0:         // first right
            case 5:         // second right
                Direction = "Right";
                break;
            case 1:         // first across
            case 4:         // second across
            case 6:         // optional
                Direction = "Across";
                break;
            case 2:         // first left
            case 3:         // second left
                Direction = "Left";
                break;
            default:
                Direction = "Done";
                break;
        }
    }

    void AIMechanics()
    {
        foreach (int key in GManager.PlayerDict.Keys)
        {
            if (GManager.PlayerDict[key] == PlayerRef.None)
            {
                ChooseTilesForAI(key);
                PlayersReady++;
            }
        }
    }

    void ChooseTilesForAI(int rackID)
    {
        // for now just choose the first three.
        // TODO: Make the AI smart
        PassArr[rackID] = GManager.Racks[rackID]
                         .GetRange(0, 3)
                         .Select(tileGO => tileGO.GetComponent<Tile>().ID)
                         .ToArray();
    }
}
