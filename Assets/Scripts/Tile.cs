using UnityEngine;
using System;
using UnityEngine.UI;

[System.Serializable]
public class Tile : IComparable<Tile>
{
    public Kind kind;
    public Suit? suit;
    public int? value;
    public Direction? direction;
    public bool isVirtual;
    public TileComponent tileComponent;

    public int ID;

    public Tile(TileComponent tc, int id, int? v = null, Suit? s = null, Direction? dir = null, bool virt = false)
    {
        tileComponent = tc;
        tileComponent.tile = this;
        ID = id;
        isVirtual = virt;

        // Numbers and Dragons
        if (v != null)
        {
            kind = v == 0 ? Kind.dragon : Kind.number;
            value = v;
            suit = s;
        }
        // Flowers and Winds
        else if (dir != null)
        {
            kind = Kind.flowerwind;
            direction = dir;
        }
        // Jokers
        else kind = Kind.joker;

        tileComponent.Init();
    }


    /*
    // NumberDragon OVERLOAD
    public Tile(int v, Suit s, bool virt = false)
    {
        kind = v == 0 ? Kind.dragon : Kind.number;
        value = v;
        suit = s;
        isVirtual = virt;
    }

    // FlowerWind OVERLOAD
    public Tile(Direction dir, bool virt = false)
    {
        kind = Kind.flowerwind;
        direction = dir;
        isVirtual = virt;
    }

    // Joker OVERLOAD
    public Tile(bool virt = false)
    {
        kind = Kind.joker;
        isVirtual = virt;
    }
    */

    public bool IsJoker() { return kind == Kind.joker; }
    public static bool IsJoker(int tileID) { return tileID >= 144; }

    public override int GetHashCode()
    {
        return HashCode.Combine(kind, suit, value, direction);
    }

    public override bool Equals(object obj)
    {
        // base checks
        if (this == obj) return true;
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;

        Tile that = (Tile)obj;
        // value checks
        if (this.kind != that.kind) return false;
        if (this.suit != that.suit) return false;
        if (this.value != that.value) return false;
        if (this.direction != that.direction) return false;

        // if everything above passed, then return true
        return true;
}

    public int CompareTo(Tile that)
    {
        if (this.Equals(that)) return 0;
        return ID.CompareTo(that.ID);
    }
}
