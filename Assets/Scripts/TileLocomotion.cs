using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
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
    private Transform RackPrivateTF;

    private void Awake()
    {
        Refs = GetComponentInParent<Tile>().Refs;
        TManager = Refs.EventSystem.GetComponent<TileManager>();
        ESystem = Refs.EventSystem.GetComponent<EventSystem>();
        TileTF = transform.parent;
        HLG = TileTF.parent.GetComponent<HorizontalLayoutGroup>();
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
    }

    public void OnSelect(BaseEventData eventData) { }
    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {   
        // update position to match mouse
        transform.position += (Vector3)eventData.delta;
    }

    private void ShiftTiles(PointerEventData eventData, GameObject overTile)
    {
        int TileTFIdx = TManager.LocalTiles.IndexOf(TileTF.gameObject);
        int overTileIdx = TManager.LocalTiles.IndexOf(overTile);

        // if to the left of original position, left side of tiles rule
        if (TileTFIdx > overTileIdx)
        {
            if (eventData.position.x > overTile.transform.position.x) { }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<RaycastResult> raycastResults = new();
        IEnumerable<GameObject> raycastGameObjects;
        ESystem.RaycastAll(eventData, raycastResults);
        raycastGameObjects = raycastResults.Select(res => res.gameObject);
        float dropXPos = eventData.pointerDrag.transform.position.x;
        // The reason for the dropXPos nonsense above is that if I just take the 
        // mouse position, things don't work as intuitively. Better to work with
        // the dragging tile's position.

        // if position is on rack, drop the tile at new position
        if (raycastGameObjects.Contains(RackPrivateTF.gameObject))
        {
            float thisDist;
            float prevDist = float.MaxValue;
            int closestTileIndex = RackPrivateTF.childCount - 1;
            Transform closestTileTF;
            int newIndex;

            // subtract current position from the position of each tile starting
            // from the left. this value will decrease and then start increasing.
            // when it starts increasing, then the previous tile was the closest
            // one. save off that index to closesttileindex.
            // if the increase is never reached, then the tile should go to the
            // end of the rack, which is why closesttileindex is initialized there.
            foreach (Transform childTF in RackPrivateTF)
            {
                thisDist = Math.Abs(dropXPos - childTF.position.x);
                if (thisDist > prevDist)
                {
                    closestTileIndex = childTF.GetSiblingIndex() - 1;
                    break;
                }
                prevDist = thisDist;
            }

            // now check if we're to the left or right of the tile and set accordingly.
            closestTileTF = RackPrivateTF.GetChild(closestTileIndex);
            if (dropXPos > closestTileTF.position.x)
            {
                newIndex = closestTileIndex + 1;
            }
            else
            {
                newIndex = closestTileIndex;
            }
            // offset one if we're moving to the right.
            if (dropXPos > TileTF.transform.position.x)
            {
                newIndex--;
            }

            TileTF.SetSiblingIndex(newIndex);
        }

        // now update the position of the front.
        transform.position = TileTF.position;
    }
}
