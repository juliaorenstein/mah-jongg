using UnityEngine;

public class Tile : MonoBehaviour
{
    // GAME OBJECTS
    public ObjectReferences Refs;
    public Transform BackTF;
    public Transform FrontTF;

    public int ID;

    private void Awake()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        BackTF = transform.GetChild(0);
        FrontTF = transform.GetChild(1);
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

    public void ShowFront()
    {
        FrontTF.gameObject.SetActive(true);
        BackTF.gameObject.SetActive(false);
    }

    public void ShowBack()
    {
        FrontTF.gameObject.SetActive(false);
        BackTF.gameObject.SetActive(true);
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
