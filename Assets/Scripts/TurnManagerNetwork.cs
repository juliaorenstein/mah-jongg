using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TurnManagerNetwork : NetworkBehaviour
{
    public ObjectReferences Refs;
    private GameManager GManager;
    private TurnManager TManager;

    [Networked]
    public int TurnPlayerID { get; set; }

    private void Awake()
    {
        GManager = GetComponent<GameManager>();
        TManager = GetComponent<TurnManager>();
        TurnPlayerID = GManager.Dealer;

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShowTileInDiscard(int tileID)
    { //ShowTileInDiscard(GManager.TileList[tileID].transform.parent);
    }
}
