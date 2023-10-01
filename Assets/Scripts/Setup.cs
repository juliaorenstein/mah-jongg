using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Setup : NetworkBehaviour
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    private TileManager TManager;
    private Transform TilePool;
    private Transform LocalRackPrivateTF;

    // PREFABS
    private GameObject TilePrefab;

    // OTHER FIELDS
    public int TileID;
    private List<GameObject> tileList;      
    public IList<GameObject> TileList;      // Everyone sees this list to know what tiles exist
    public Stack<GameObject> Wall;          // Host sees this stack to deal and draw
    private List<List<GameObject>> Racks;   // Host deals out to racks, each client sees one
    private List<GameObject> LocalTiles;     // Each client has their own rack in this variable

    void Awake()
    {   // INITIALIZE FIELDS
        TileID = 0;
        tileList = new();
        Racks = new();
        LocalTiles = Refs.EventSystem.GetComponent<TileManager>().LocalTiles;
        TilePool = Refs.TilePool.transform;
        LocalRackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TilePrefab = Resources.Load<GameObject>("Prefabs/Tile");
    }

    public void SetupGame(NetworkRunner runner, PlayerRef player)
    {
        int[] tilesToSend;

        // FIRST LOCAL PLAYER CREATES TILES
        CreateTiles();

        // SERVER & LOCAL PLAYER: SHUFFLE AND DEAL
        if (runner.IsServer && runner.LocalPlayer == player)
        {
            Shuffle();
            Deal();
            // new comment
        }

        // SERVER: POPULATE LOCAL PLAYER'S RACK
        if (runner.IsServer)
        {
            tilesToSend = PrepRack(player.PlayerId);
            RpcInvokeInfo info = RPC_SendRackToPlayer(player, tilesToSend);
        }

        // LOCAL PLAYER: TURN OFF START BUTTON
        if (runner.LocalPlayer == player)
        {
            Refs.StartButton.SetActive(false);
        }
    }

    public void CreateTiles()
    {
        CreateNumberDragons();
        CreateFlowerWinds();
        CreateJokers();

        // CREATE REFERENCE LIST FOR EVERYONE
        TileList = tileList.AsReadOnly();
    }

    Tile CreateOneTile()
    {
        Tile tile = Instantiate(TilePrefab, TilePool).GetComponent<Tile>();
        tile.GetComponent<Tile>().ID = TileID;
        tileList.Add(tile.gameObject);
        TileID++;

        return tile.GetComponent<Tile>();
    }

    void CreateNumberDragons()
    {
        foreach (string suit in NumberDragon.SuitList)
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
        foreach (string dir in FlowerWind.DirectionList)
        {
            for (int id = 1; id < 5; id++)
            {
                CreateOneTile().InitTile(dir);
            }
        }

        // AND THE LAST FOUR FLOWERS
        for (int id = 5; id < 9; id++)
        {
            CreateOneTile().InitTile("Flower");
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
        Wall = new(shuffleTileList);
    }

    public void Deal()
    {
        List<GameObject> rack;

        for (int i = 0; i < 4; i++)
        {   // ADD FOUR RACKS TO RACKS FIELD
            rack = new();
            Racks.Add(rack);

            for (int j = 0; j < 13; j++)
            {   // ADD THIRTEEN TILES TO EACH RACK
                rack.Add(Wall.Pop());
            }    
        }

        // DEAL ONE MORE TILE TO THE DEALER
        Racks[3].Add(Wall.Pop());
    }

    // PREPARE A LIST OF TILE IDS TO SEND TO CLIENT FOR RPC_SendRackToPlayer
    int[] PrepRack(int playerID)
    {
        List<int> RackTileIDs = new();
        foreach (GameObject tile in Racks[playerID])
        {
            RackTileIDs.Add(tile.GetComponent<Tile>().ID);
        }
        return RackTileIDs.ToArray();
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public RpcInvokeInfo RPC_SendRackToPlayer([RpcTarget] PlayerRef player, int[] tileArr)
    {
        Debug.Log("RPC");
        GameObject tile;
        foreach (int tileID in tileArr)
        {
            tile = TileList[tileID];
            LocalTiles.Add(tile);
            tile.transform.SetParent(LocalRackPrivateTF);
            tile.GetComponent<Tile>().ShowFront();
        }
        return default;
    }    
}
