using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileLocomotion : MonoBehaviour
    , ISelectHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    private ObjectReferences Refs;
    private TileManager TManager;
    private EventSystem ESystem;
    private Transform TileTF;
    private HorizontalLayoutGroup HLG;
    private Vector3 origPos;


    private IEnumerable<GameObject> raycastGameObjects;
    List<GameObject> overTileList;

    private List<RaycastResult> raycastResults = new();

    private void Awake()
    {
        Refs = GetComponentInParent<Tile>().Refs;
        TManager = Refs.EventSystem.GetComponent<TileManager>();
        ESystem = Refs.EventSystem.GetComponent<EventSystem>();
        TileTF = transform.parent;
        HLG = TileTF.parent.GetComponent<HorizontalLayoutGroup>();
    }

    public void OnSelect(BaseEventData eventData)
    {
         
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //origPos = eventData.pressPosition;
    }




    public void OnDrag(PointerEventData eventData)
    {   
        // UPDATE POSITION TO MATCH MOUSE
        transform.position += (Vector3)eventData.delta;

        // IF THE POSITION IS BETWEEN TWO OTHER TILES, SHIFT THEM
        /*
        ESystem.RaycastAll(eventData, raycastResults);
        overTileList = raycastResults
            .Select(res => res.gameObject)
            .Intersect(TManager.LocalTiles).ToList();
        if (overTileList.Count > 0) ShiftTiles(eventData, overTileList[0]);
        */
    }

    private void ShiftTiles(PointerEventData eventData, GameObject overTile)
    {
        GameObject draggingTile = eventData.pointerDrag;
        int draggingTileIdx = TManager.LocalTiles.IndexOf(draggingTile);
        int overTileIdx = TManager.LocalTiles.IndexOf(overTile);

        // IF TO THE LEFT OF ORIGINAL POSITION, LEFT SIDE OF TILES RULE
        if (draggingTileIdx > overTileIdx)
        {
            if (eventData.position.x > overTile.transform.position.x)
            {
                //spotTF = Rack.RackPosToSpot();
                // TODO: Before you continue here, make "MyRack" and make it always the bottom rack. Much simpler.
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TileTF.SetSiblingIndex(TileTF.parent.childCount - 1);
        transform.position = TileTF.position;
        //HLG.CalculateLayoutInputHorizontal();
        HLG.SetLayoutHorizontal();
        HLG.SetLayoutVertical();
    }
}
