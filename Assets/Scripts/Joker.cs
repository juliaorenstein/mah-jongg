using UnityEngine;
using UnityEngine.UI;

public class Joker : MonoBehaviour, ITile
{
    private Tile T;

    private void Awake()
    {
        T = GetComponent<Tile>();
    }

    public string SetName()
    {
        string name = "Joker";
        gameObject.name = name;
        return name;
    }

    public void SetFace(string name)
    {
        T.FaceTF.GetComponent<Image>().sprite
            = Resources.Load<Sprite>("Tile Faces/Joker");
    }
}
