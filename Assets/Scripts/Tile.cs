using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class Tile : IComparable<Tile>
{
    public Kind kind;
    public Suit? suit;
    public int? value;
    public Direction? direction;
    public bool isVirtual;
    public TileComponent tileComponent;

    public int ID;

    public Tile(int? v = null, Suit? s = null, Direction? dir = null, bool virt = true)
    {
        SetValues(v, s, dir);
    }

    public Tile(TileComponent tc, int id, int? v = null, Suit? s = null, Direction? dir = null, bool virt = false)
    {
        tileComponent = tc;
        tileComponent.tile = this;
        ID = id;
        isVirtual = virt;

        SetValues(v, s, dir);

        tileComponent.Init();
    }

    void SetValues(int? v = null, Suit? s = null, Direction? dir = null)
    {
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

    public override string ToString()
    {
        switch (kind)
        {
            case Kind.flowerwind:
                return direction.ToString();
            case Kind.number:
                return $"{value} {suit}";
            case Kind.dragon:
                switch (suit)
                {
                    case Suit.bam:
                        return "Green";
                    case Suit.crak:
                        return "Red";
                    case Suit.dot:
                        return "Soap";
                    default:
                        throw new Exception("invalid dragon suit");
                }
            case Kind.joker:
                return "Joker";
            default:
                throw new Exception("invalid kind");
        }
    }
}
