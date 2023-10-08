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
    }

    public void MoveTile(Transform toLoc, int toPos = -1)
    {   // TODO: REWRITE
        /*
        if (toLoc.parent == RacksTF)
        {
            MoveTileToRack(toLoc, toPos);
        }

        else if (toLoc == WallTF)
        {
            MoveTileToDiscard(toLoc);
        }
        */
    }

    private void MoveTileToRack(Transform rack, int toPos)
    {
        if (toPos < 0) toPos = rack.childCount;
        transform.SetParent(rack);
        transform.localScale = new Vector3(1, 1, 1);
        //transform.position = Rack.RackPosToSpot(rack.GetSiblingIndex(), toPos);
    }

    private void MoveTileToDiscard(Transform wall)
    {
        transform.SetParent(wall);
    }

    private void MoveTileToCharleston()
    {
        return;
    }
}
