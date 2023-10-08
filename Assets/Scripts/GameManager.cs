using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class GameManager : MonoBehaviour
{
    public bool Offline = false;
    public int Dealer;
    public IList<GameObject> TileList;
    public Dictionary<int, PlayerRef> PlayerDict = new();
    public List<List<GameObject>> Racks = new();

    private void Awake()
    {
        for (int playerID = 0; playerID < 4; playerID++)
        {
            PlayerDict[playerID] = PlayerRef.None;
        }
    }
}

