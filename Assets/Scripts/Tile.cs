using UnityEngine;
using System;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    public Transform FaceTF;
    public Kind kind;
    public Suit? suit;
    public int? value;
    public Direction? direction;

    public int ID;

    private void Awake()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        FaceTF = transform.GetChild(0);
    }

    // NumberDragon OVERLOAD
    public void InitTile(int val, Suit sui)
    {
        kind = val == 0 ? Kind.dragon : Kind.number;
        value = val;
        suit = sui;
        /*
        NumberDragon nd = gameObject.AddComponent<NumberDragon>();
        nd.Number = num;
        nd.Suit = suit;
        */
        FinishInit();
    }

    // FlowerWind OVERLOAD
    public void InitTile(Direction dir)
    {
        kind = Kind.flowerwind;
        direction = dir;
        /*
        FlowerWind fw = gameObject.AddComponent<FlowerWind>();
        fw.Direction = dir;
        */
        FinishInit();
    }

    // Joker OVERLOAD
    public void InitTile()
    {
        kind = Kind.joker;
        /*
        gameObject.AddComponent<Joker>();
        */
        FinishInit();
    }

    public void FinishInit()
    {
        string name = GetComponent<ITile>().SetName();
        GetComponent<ITile>().SetFace(name);
        transform.GetChild(0).name = gameObject.name + " face";
    }

    // just points over to child's TileLocomotion component's MoveTile
    public void MoveTile(Transform toLoc)
    {
        GetComponentInChildren<TileLocomotion>().MoveTile(toLoc);
    }

    public bool IsJoker() { return name == "Joker"; }
    public static bool IsJoker(int tileID) { return tileID >= 144; }

    public int CompareTo(Tile other) { return ID.CompareTo(other.ID); }
}
