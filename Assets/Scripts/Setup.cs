using System;
using System.Collections.Generic;
using System.IO;
using Fusion;
using UnityEngine;


public class Setup : NetworkBehaviour, ISetup
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    private GameManager GManager;
    private Transform TilePool;
    private Transform OtherRacksTF;
    private Transform LocalRackPrivateTF;
    private CharlestonPassButton PassButtonScript;

    // PREFABS
    private GameObject TileBackPF;
    private GameObject InputObjectPF;

    // OTHER FIELDS
    public int TileID;
    private List<GameObject> tileList;      
    private List<GameObject> LocalTiles;    // Each client has their own rack in this variable

    public override void Spawned()
    {           
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        Refs.Managers = gameObject;
        PassButtonScript = Refs.Charleston.GetComponentInChildren<CharlestonPassButton>();
        GManager = GetComponent<GameManager>();
        PassButtonScript.GManager = GManager;
        PassButtonScript.CManager = Refs.Managers.GetComponent<CharlestonManager>();
        TileID = 0;
        tileList = new();
        LocalTiles = Refs.Managers.GetComponent<TileManager>().LocalTiles;
        TilePool = Refs.TilePool.transform;
        LocalRackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        OtherRacksTF = Refs.OtherRacks.transform;
        TileBackPF = Resources.Load<GameObject>("Prefabs/Tile Back");
        InputObjectPF = Resources.Load<GameObject>("Prefabs/Input Object");
        foreach (Transform tileTF in TilePool)
        {
            tileList.Add(tileTF.gameObject);
            // create reference list for everyone
            GameManager.TileList = tileList.AsReadOnly();
            // add locomotive abilities
            tileTF.GetChild(0).gameObject.AddComponent<TileLocomotion>();
        }
        C_Setup();
    }

    public void C_Setup()
    {
        GManager.LocalPlayerID = Runner.LocalPlayer.PlayerId; // set player ID
        GManager.DealerID = 3;                    // make the server the dealer
        
        Refs.Charleston.transform.SetParent(Refs.Board.transform);
        HideButtons();                      // hide start buttons
        PopulateOtherRacks();               // show the other player's racks
        
        if (Runner.IsServer)    // one time actions when the host joins
        {
            Shuffle();
            Deal();
        }
    }

    public void H_Setup(PlayerRef player)
    {
        NetworkObject newInputObj = Runner.Spawn(InputObjectPF);
        GManager.PlayerDict[player.PlayerId] = player;
        GManager.InputDict.Add(player.PlayerId, newInputObj.GetComponent<InputCollection>());
        newInputObj.AssignInputAuthority(player);
        int[] tileArr = PrepRackForClient(player.PlayerId);
        RPC_SendRackToPlayer(player, tileArr);
    }

    // FISHER-YATES
    public void Shuffle()
    {
        List<GameObject> shuffleTileList = new(tileList);
        GameObject tmp;
        int k;

        System.Random rnd = new();

        for (int i = shuffleTileList.Count - 1; i > 0; i--)
        {
            k = rnd.Next(i);
            tmp = shuffleTileList[k];
            shuffleTileList[k] = shuffleTileList[i];
            shuffleTileList[i] = tmp;
        }

        // CREATE THE WALL
        GManager.Wall = new(shuffleTileList);
    }

    public void Deal()
    {
        List<GameObject> rack;

        for (int i = 0; i < 4; i++)
        {   // ADD FOUR RACKS TO RACKS FIELD
            rack = new();
            GManager.Racks.Add(rack);

            for (int j = 0; j < 13; j++)
            {   // ADD THIRTEEN TILES TO EACH RACK
                rack.Add(GManager.Wall.Pop());
            }    
        }

        // DEAL ONE MORE TILE TO THE DEALER
        GManager.Racks[GManager.DealerID].Add(GManager.Wall.Pop());
    }

    // PREPARE A LIST OF TILE IDS TO SEND TO CLIENT FOR RPC_SendRackToPlayer
    int[] PrepRackForClient(int playerID)
    {
        List<int> RackTileIDs = new();
        foreach (GameObject tile in GManager.Racks[playerID])
        {
            RackTileIDs.Add(tile.GetComponent<TileComponent>().tile.ID);
        }
        return RackTileIDs.ToArray();
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_SendRackToPlayer([RpcTarget] PlayerRef _, int[] tileArr)
    {
        PopulateLocalRack(tileArr);
    }

    void PopulateLocalRack(int[] tileArr)
    {
        GameObject tile;
        foreach (int tileID in tileArr)
        {
            tile = GameManager.TileList[tileID];
            LocalTiles.Add(tile);
            TileLocomotion.MoveTile(tile.transform, LocalRackPrivateTF);
        }
    }

    void HideButtons()
    {
        Refs.StartButtons.SetActive(false);
    }

    void PopulateOtherRacks()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 13; j++)
            { Instantiate(TileBackPF, OtherRacksTF.GetChild(i).GetChild(1)); }
        }

        // one more tile for the dealer if this isn't the server/dealer
        if (GManager.DealerID != GManager.LocalPlayerID)
        { Instantiate(TileBackPF, OtherRacksTF.GetChild(0).GetChild(1)); }
    }
}
