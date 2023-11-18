using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

public class ObjectReferences : MonoBehaviour
{
    public GameObject Managers;
    public GameManager GameManager;
    public TurnManager TurnManager;
    public EventSystem EventSystem;
    public NetworkCallbacks NetworkCallbacks;
    public NetworkRunner Runner;
    public GameObject DealMe;
    public GameObject Board;
    public GameObject TilePool;
    public GameObject LocalRack;
    public GameObject OtherRacks;
    public GameObject StartButtons;
    public GameObject Dragging;
    public Transform Charleston;
    public GameObject Discard;
    public GameObject TurnIndicator;
    public GameObject CallWaitButtons;
}
