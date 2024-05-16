using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

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

        FinishInit();
        return this;
    }

    // FlowerWind OVERLOAD
    public Tile InitTile(Direction dir)
    {
        kind = Kind.flowerwind;
        direction = dir;

        FinishInit();
        return this;
    }

    // Joker OVERLOAD
    public Tile InitTile()
    {
        kind = Kind.joker;

        FinishInit();
        return this;
    }

    public void FinishInit()
    {
        SetName();
        SetFace();
        transform.GetChild(0).name = gameObject.name + " face";
    }


    void SetName()
    {
        switch (kind)
        {
            case Kind.flowerwind:
                SetFlowerWindName();
                break;
            case Kind.number:
                SetNumberName();
                break;
            case Kind.dragon:
                SetDragonName();
                break;
            case Kind.joker:
                SetJokerName();
                break;
            default:
                break;
        };
    }

    void SetFlowerWindName()
    { gameObject.name = Enum.GetName(typeof(Direction), direction); }

    void SetNumberName()
    { gameObject.name = $"{value} {suit}"; }

    void SetDragonName()
    {
        gameObject.name = suit switch
        {
            Suit.bam => "Green",
            Suit.crak => "Red",
            Suit.dot => "Soap",
            _ => "Dragon - error",
        };
    }

    void SetJokerName()
    { gameObject.name = "Joker"; }


    public void SetFace()
    {
        string spriteName;

        if (gameObject.name == "flower")
            spriteName = ID switch
            {
                136 => "Spring", 137 => "Summer", 138 => "Autumn",
                139 => "Winter", 140 => "Bamboo", 141 => "Chrys",
                142 => "Orchid", 143 => "Plumb", _ => "Error"
            };

        else { spriteName = name; }

        GetComponentInChildren<Image>().sprite
                = Resources.Load<Sprite>($"Tile Faces/{spriteName}");
    }

    // just points over to child's TileLocomotion component's MoveTile
    public void MoveTile(Transform toLoc)
    {
        GetComponentInChildren<TileLocomotion>().MoveTile(toLoc);
    }

    public bool IsJoker() { return name == "Joker"; }
    public static bool IsJoker(int tileID) { return tileID >= 144; }

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
