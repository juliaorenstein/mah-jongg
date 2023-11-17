using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using System;

public class GameManager : MonoBehaviour
{
    public ObjectReferences Refs;
    public int LocalPlayerID;
    public int DealerID;
    public static IList<GameObject> TileList;
    public Dictionary<int, PlayerRef> PlayerDict = new();
    public Dictionary<int, InputCollection> InputDict = new();
    public List<List<GameObject>> Racks = new();
    public Stack<GameObject> Wall = new();
    public int WaitTime;

    private void Awake()
    {
        for (int playerID = 0; playerID < 4; playerID++)
        {
            PlayerDict[playerID] = PlayerRef.None;
        }

        WaitTime = 2000; // players have 2 seconds to call a tile (or say "wait")
    }
}

