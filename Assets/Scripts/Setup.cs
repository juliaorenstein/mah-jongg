using System.Collections.Generic;
using System.Threading;
using Fusion;
using UnityEngine;


public class Setup : NetworkBehaviour
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    private GameManager GManager;
    private NetworkRunner _runner;
    private Transform TilePool;
    private Transform OtherRacksTF;
    private Transform LocalRackPrivateTF;
    private TurnManager TManager;

    // PREFABS
    private GameObject TilePF;
    private GameObject TileBackPF;

    // OTHER FIELDS
    public int TileID;
    private List<GameObject> tileList;      
    private List<GameObject> LocalTiles;    // Each client has their own rack in this variable

    void Awake()
    {   // INITIALIZE FIELDS
        TileID = 0;
        tileList = new();
        GManager = Refs.GameManager.GetComponent<GameManager>();
        LocalTiles = Refs.EventSystem.GetComponent<TileManager>().LocalTiles;
        TilePool = Refs.TilePool.transform;
        LocalRackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        OtherRacksTF = Refs.OtherRacks.transform;
        TilePF = Resources.Load<GameObject>("Prefabs/Tile");
        TileBackPF = Resources.Load<GameObject>("Prefabs/Tile Back");
        TManager = GManager.GetComponent<TurnManager>();
    }

    public void SetupGame(NetworkRunner runner, PlayerRef player)
    {
        _runner = runner;
        GManager.PlayerDict[player.PlayerId] = player;
        GManager.Dealer = 3;                    // make the server the dealer
        GManager.LocalPlayerID = runner.LocalPlayer.PlayerId; // set player ID
        if (GManager.Dealer == GManager.LocalPlayerID)
        CreateTiles();                          // everyone creates the tiles

        if (_runner.IsServer && GManager.LocalPlayerID == player)
        {
            Shuffle();
            Deal();
        }

        if (_runner.IsServer)  
        {                                       // server deals to clients
            int[] tileArr = PrepRackForClient(player.PlayerId);
            RPC_SendRackToPlayer(player, tileArr);
        }

        if (_runner.LocalPlayer == player)           
        {
            HideButtons();                      // hide start buttons
            PopulateOtherRacks();               // show the other player's racks
        }
    }

    public void SetupOfflineGame()
    {
        GManager.Dealer = 0;                    // set dealer
        GManager.Offline = true;                // this check will be handy
        CreateTiles();                          // create tiles
        Shuffle();                              // shuffle
        Deal();                                 // deal
        int[] tileArr = PrepRackForClient(3);   // get tiles on rack
        PopulateLocalRack(tileArr);             // populate 'em
        HideButtons();                          // hide start buttons
        PopulateOtherRacks();                   // show the other player's racks
    }

    public void CreateTiles()
    {
        CreateNumberDragons();
        CreateFlowerWinds();
        CreateJokers();

        // create reference list for everyone
        GManager.TileList = tileList.AsReadOnly();
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
        GManager.Racks[3].Add(GManager.Wall.Pop());
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
            tile = GManager.TileList[tileID];
            LocalTiles.Add(tile);
            tile.transform.SetParent(LocalRackPrivateTF);
        } 
    }

    void HideButtons()
    {
        Refs.StartButtons.SetActive(false);
    }

    void PopulateOtherRacks()
    {
        // TODO: Remake tile prefab and fix references so that we can just have the front
        // FIXME: Tiles are too big and stretching the otherracks vertically

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Instantiate(TileBackPF, OtherRacksTF.GetChild(i).GetChild(1));
            }
        }

        // one more tile for the dealer if this isn't the server/dealer
        if ((GManager.Offline && GManager.Dealer < 3)
            || GManager.Dealer == GManager.LocalPlayerID)
        {
            Instantiate(TileBackPF, OtherRacksTF.GetChild(0).GetChild(1));
        }
    }
}
