using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class GameManager : MonoBehaviour
{
    public ObjectReferences Refs;
    public bool Offline = false;
    public int LocalPlayerID;
    public int Dealer;
    public static IList<GameObject> TileList;
    public Dictionary<int, PlayerRef> PlayerDict = new();
    public List<List<GameObject>> Racks = new();
    public Stack<GameObject> Wall = new();

    private void Awake()
    {
        for (int playerID = 0; playerID < 4; playerID++)
        {
            PlayerDict[playerID] = PlayerRef.None;
        }
    }
}

