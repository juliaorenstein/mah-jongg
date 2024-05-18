using System;
using UnityEngine;
using UnityEngine.UI;

public class TileComponent : MonoBehaviour
{
    // GAME OBJECTS
    public Transform FaceTF;
    public Tile tile;

    private void Awake()
    {
        FaceTF = transform.GetChild(0);
    }

    public void Init()
    {
        SetName();
        SetFace();
        transform.GetChild(0).name = gameObject.name + " face";
    }


    void SetName()
    {
        switch (tile.kind)
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

        void SetFlowerWindName()
        { gameObject.name = Enum.GetName(typeof(Direction), tile.direction); }

        void SetNumberName()
        { gameObject.name = $"{tile.value} {tile.suit}"; }

        void SetDragonName()
        {
            gameObject.name = tile.suit switch
            {
                Suit.bam => "Green",
                Suit.crak => "Red",
                Suit.dot => "Soap",
                _ => "Dragon - error",
            };
        }

        void SetJokerName()
        { gameObject.name = "Joker"; }
    }

    public void SetFace()
    {
        string spriteName;

        if (gameObject.name == "flower")
            spriteName = tile.ID switch
            {
                136 => "Spring",
                137 => "Summer",
                138 => "Autumn",
                139 => "Winter",
                140 => "Bamboo",
                141 => "Chrys",
                142 => "Orchid",
                143 => "Plumb",
                _ => "Error"
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
}
