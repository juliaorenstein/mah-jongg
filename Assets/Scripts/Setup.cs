using System;
using System.Collections.Generic;
using System.IO;
using Fusion;
using UnityEngine;


public class Setup : NetworkBehaviour
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    private GameManager GManager;
    private Transform TilePool;
    private Transform OtherRacksTF;
    private Transform LocalRackPrivateTF;
    private CharlestonPassButton PassButtonScript;

    // PREFABS
    private GameObject TilePF;
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
        TilePF = Resources.Load<GameObject>("Prefabs/Tile");
        TileBackPF = Resources.Load<GameObject>("Prefabs/Tile Back");
        InputObjectPF = Resources.Load<GameObject>("Prefabs/Input Object");
        C_Setup();
    }

    public void C_Setup()
    {
        GManager.LocalPlayerID = Runner.LocalPlayer.PlayerId; // set player ID
        GManager.DealerID = 3;                    // make the server the dealer
        
        Refs.Charleston.transform.SetParent(Refs.Board.transform);
        CreateTiles();
        //WriteTilesToFile();
        HideButtons();                      // hide start buttons
        PopulateOtherRacks();               // show the other player's racks
        
        if (Runner.IsServer)    // one time actiosns when the host joins
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

    void CreateTiles()
    {
        CreateNumberDragons();
        CreateFlowerWinds();
        CreateJokers();

        // create reference list for everyone
        GameManager.TileList = tileList.AsReadOnly();
    }

    // for debugging
    void WriteTilesToFile()
    {
        string filePath = "/Users/juliaorenstein/Unity/Maj/Tiles.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (GameObject tileGO in GameManager.TileList)
                {
                    Tile tile = tileGO.GetComponent<Tile>();
                    string line = $"{tile.ID},{tile.name}";
                    // Write to the file
                    writer.WriteLine(line);
                }
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }

        
    }

    Tile CreateOneTile()
    {
        Tile tile = Instantiate(TilePF, TilePool).GetComponent<Tile>();
        tile.GetComponent<Tile>().ID = TileID;
        tileList.Add(tile.gameObject);
        TileID++;

        return tile.GetComponent<Tile>();
    }

    void CreateNumberDragons()
    {
        Suit[] suits = (Suit[])Enum.GetValues(typeof(Suit));

        foreach (Suit suit in suits)
        {
            for (int num = 0; num < 10; num++)
            {
                for (int i = 1; i < 5; i++)
                {
                    CreateOneTile().InitTile(num, suit);
                }
            }
        }
    }

    void CreateFlowerWinds()
    {
        Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));

        foreach (Direction dir in directions)
        {
            for (int id = 1; id < 5; id++)
            {
                CreateOneTile().InitTile(dir);
            }
        }

        // AND THE LAST FOUR FLOWERS
        for (int id = 5; id < 9; id++)
        {
            CreateOneTile().InitTile(Direction.flower);
        }
    }

    void CreateJokers()
    {
        for (int id = 1; id < 9; id++)
        {
            CreateOneTile().InitTile();
        }
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
            RackTileIDs.Add(tile.GetComponent<Tile>().ID);
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
