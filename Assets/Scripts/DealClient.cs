using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

public class DealClient : NetworkBehaviour
    , IPointerClickHandler
{
    public ObjectReferences Refs;
    private Setup Setup;
    private NetworkRunner NRunner;
    public PlayerRef Player;

    private void Awake()
    {
        Setup = Refs.GameManager.GetComponent<Setup>();
        NRunner = Refs.Runner.GetComponent<NetworkRunner>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        Setup.SetupGame(NRunner, Player);
    }
    /*
    [Rpc]
    public void RPC_Test()
    {
        Debug.Log("RPC");
    }
    */
}
