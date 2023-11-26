using UnityEngine;
using System;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    // GAME OBJECTS
    public Transform FaceTF;
    public Kind kind;
    public Suit? suit;
    public int? value;
    public Direction? direction;

    public int ID;

    private void Awake()
    {
        FaceTF = transform.GetChild(0);
    }

    // NumberDragon OVERLOAD
    public Tile InitTile(int val, Suit sui)
    {
        kind = val == 0 ? Kind.dragon : Kind.number;
        value = val;
        suit = sui;
        gameObject.AddComponent<NumberDragon>();
        
        FinishInit();
        return this;
    }

    // FlowerWind OVERLOAD
    public Tile InitTile(Direction dir)
    {
        kind = Kind.flowerwind;
        direction = dir;
        
        gameObject.AddComponent<FlowerWind>();
        
        FinishInit();
        return this;
    }

    // Joker OVERLOAD
    public Tile InitTile()
    {
        kind = Kind.joker;
        
        gameObject.AddComponent<Joker>();
        FinishInit();
        return this;
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
