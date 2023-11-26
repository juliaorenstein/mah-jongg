using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowerWind : MonoBehaviour, ITile
{
    public Direction Direction;
    private Tile T;

    private void Awake()
    {
        T = GetComponent<Tile>();
    }
    /*
    public static List<string> DirectionList =
        new() { "North", "South", "East", "West", "Flower" };

    public static List<string> FlowerLIst =
        new() { "Spring", "Summer", "Autumn", "Winter",
        "Bamboo", "Chrys", "Orchid", "Plumb" };
    */

    public string SetName()
    {
        string name = Direction.ToString();
        gameObject.name = name;
        return name;
    }

    public void SetFace(string name)
    {
        string spriteName;
        if (name == "Flower")
        {
            spriteName = T.ID switch
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

            T.FaceTF.GetComponent<Image>().sprite
                = Resources.Load<Sprite>($"Tile Faces/{spriteName}");
        }

        else
        {
            spriteName = name;
            T.FaceTF.GetComponent<Image>().sprite
                = Resources.Load<Sprite>($"Tile Faces/{spriteName}");
        }
    }
}
