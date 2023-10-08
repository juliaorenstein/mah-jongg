using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Charleston : NetworkBehaviour
{
    public ObjectReferences Refs;
    private Button PassButton;
    private GameManager GManager;
    private Transform RackPrivateTF;
    private Transform TilePoolTF;
    private int[] TilesToPass = new int[3];
    private int[][] PassArr = new int[4][];
    private int Counter = 0;
    private int PlayersReady = 0;

    private void Awake()
    {
        PassButton = Refs.CharlestonPassButton.GetComponent<Button>();
        GManager = Refs.GameManager.GetComponent<GameManager>();
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TilePoolTF = Refs.TilePool.transform;
    }

    private void Start()
    {   // if playing online, everybody except local player is ready to pass
        if (GManager.Offline) { PlayersReady = 3; }
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
    
    public void Pass()
    {   // on local machine, put pass tiles back into the TilePool
        // kick off the rest of the pass via RPC

        // TODO: account for optional charlestons

        for (int i = 0; i < 3; i++)     
        {
            Tile tile = transform.GetChild(i).GetChild(0).GetComponent<Tile>();
            TilesToPass[i] = tile.ID;   // set array of tiles to pass
            tile.MoveTile(TilePoolTF);  // move tile to TilePool for local player
            if (!GManager.Offline) { RPC_PlayerReady(TilesToPass); }
        }
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
        if (PlayersReady == 4) { PassOnHost(); }
    }

    void PassOnHost()
    {
        for (int sourceID = 0; sourceID < 4; sourceID++)
        {
            int targetID = PassTargetID(sourceID);
            PlayerRef targetRef = GManager.PlayerDict[targetID];
            UpdateRackLists(sourceID, targetID, PassArr[sourceID]);
            if (!GManager.Offline)
                { RPC_ReceiveTiles(targetRef, PassArr[sourceID]); }
        }
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
        foreach (int ID in tiles)
        {
            GManager.TileList[ID]
                    .GetComponent<TileLocomotion>()
                    .MoveTile(RackPrivateTF);
        }
    }

    int PassTargetID(int sourceID)
    {
        // calculate the target off the pass rpc based on the local
        // player id and the charleston counter
        int shift;

        switch (Counter)
        {
            case 0:         // first right
            case 5:         // second right
                shift = 1;
                break;
            case 1:         // first across
            case 4:         // second across
            case 6:         // optional
                shift = 2;  
                break;
            default:        // first and second left
                shift = -1; 
                break;
        }
        return (sourceID + shift) % 4;
    }
}
