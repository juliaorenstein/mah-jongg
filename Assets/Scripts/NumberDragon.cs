using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberDragon : MonoBehaviour, ITile
{
    public int Number { get; set; }
    public string Suit { get; set; }
    public static List<string> SuitList = new() { "bam", "crak", "dot" };
    private Tile T;

    private void Awake()
    {
        T = GetComponent<Tile>();
    }

    public string SetName()
    {
        string name;
        if (Number == 0)
        {
            switch (Suit)
            {
                case "bam":
                    name = "Green";
                    break;
                case "crak":
                    name = "Red";
                    break;
                case "dot":
                    name = "Soap";
                    break;
                default:
                    name = "Dragon - error";
                    break;
            }
        }

        else
        {
            name = $"{Number} {Suit}";
        }
        gameObject.name = name;
        return name;
    }

    public void SetFace(string name)
    {
        T.FrontTF.GetComponent<Image>().sprite
            = Resources.Load<Sprite>($"Tile Faces/{name}");
    } 
}
