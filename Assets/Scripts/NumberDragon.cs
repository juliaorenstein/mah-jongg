using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberDragon : MonoBehaviour, ITile
{
    public int Number { get; set; }
    public Suit Suit { get; set; }
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
            name = Suit switch
            {
                Suit.bam => "Green",
                Suit.crak => "Red",
                Suit.dot => "Soap",
                _ => "Dragon - error",
            };
        }
        else { name = $"{Number} {Suit}"; }

        gameObject.name = name;
        return name;
    }

    public void SetFace(string name)
    {
        T.FaceTF.GetComponent<Image>().sprite
            = Resources.Load<Sprite>($"Tile Faces/{name}");
    } 
}
