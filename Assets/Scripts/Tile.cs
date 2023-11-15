using UnityEngine;

public class Tile : MonoBehaviour
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    public Transform FaceTF;

    public int ID;

    private void Awake()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        FaceTF = transform.GetChild(0);
    }

    // NumberDragon OVERLOAD
    public void InitTile(int num, string suit)
    {
        NumberDragon nd = gameObject.AddComponent<NumberDragon>();
        nd.Number = num;
        nd.Suit = suit;
        FinishInit();
    }

    // FlowerWind OVERLOAD
    public void InitTile(string dir)
    {
        FlowerWind fw = gameObject.AddComponent<FlowerWind>();
        fw.Direction = dir;
        FinishInit();
    }

    // Joker OVERLOAD
    public void InitTile()
    {
        gameObject.AddComponent<Joker>();
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
}
