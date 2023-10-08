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
    private EventSystem ESystem;
    public Transform TileTF;
    private Transform RackPrivateTF;
    private Image TileImage;
    private Transform DraggingTF;
    private Transform CharlestonBoxTF;

    private void Awake()
    {
        Refs = GetComponentInParent<Tile>().Refs;
        ESystem = Refs.EventSystem.GetComponent<EventSystem>();
        TileTF = transform.parent;
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        TileImage = GetComponent<Image>();
        DraggingTF = Refs.Dragging.transform;
        CharlestonBoxTF = Refs.CharlestonBox.transform;
    }

    public void OnSelect(BaseEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        TileImage.raycastTarget = false; // To make OnDrop work - maybe get rid of this
        transform.SetParent(DraggingTF); // Keep the face in a spot lower on the
                                         // heirarchy when dragging to keep it at
                                         // the top of the UI
    }

    public void OnDrag(PointerEventData eventData)
    {   
        // update position to match mouse
        transform.position += (Vector3)eventData.delta;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(TileTF);        // undo OnBeginDrag things

        // get list of things we're on top of
        List<RaycastResult> raycastResults = new();
        ESystem.RaycastAll(eventData, raycastResults);
        IEnumerable<Transform> raycastTFs;
        raycastTFs = raycastResults.Select(res => res.gameObject.transform);
        float dropXPos = eventData.pointerDrag.transform.position.x;
        
        // if position is on rack, drop the tile at new position
        if (raycastTFs.Contains(RackPrivateTF)) { DropOnRack(dropXPos); }

        // if we're dropping to charleston (but not the border of the box), drop there.
        else if (raycastTFs.Contains(CharlestonBoxTF)
            && eventData.pointerCurrentRaycast.gameObject != CharlestonBoxTF.gameObject)
            { DropOnCharleston(eventData); }

        // otherwise, move the tile back to where it came from
        else { MoveBack(); }
        CharlestonBoxTF.GetComponent<Charleston>().CheckDone();
        TileImage.raycastTarget = true;     // undo OnBeginDrag things
    }

    void DropOnRack(float dropXPos)
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
        newIndex = dropXPos > closestTileTF.position.x ? closestTileIndex + 1 : closestTileIndex;
        // offset one if we're moving to the right.
        if (dropXPos > TileTF.transform.position.x) { newIndex--; }
        MoveTile(RackPrivateTF, newIndex);
    }

    void DropOnCharleston(PointerEventData eventData)
    {
        Transform raycastTF = eventData.pointerCurrentRaycast.gameObject.transform;
        Transform charlestonSpotTF;

        if (raycastTF.parent.CompareTag("Tile"))
        {
            charlestonSpotTF = raycastTF.parent.parent;
            raycastTF.GetComponent<TileLocomotion>().MoveTile(RackPrivateTF);
        }
        else { charlestonSpotTF = raycastTF; }
        MoveTile(charlestonSpotTF);

        
    }

    void MoveBack()
    {
        MoveTile(TileTF.parent, TileTF.GetSiblingIndex());
    }

    // will handle whether called from the tile or its face!
    public void MoveTile(Transform newParentTF, int newSibIx)
    {
        Transform tileTF;
        if (CompareTag("Tile")) tileTF = transform;
        else tileTF = transform.parent;

        tileTF.SetParent(newParentTF);
        if (newParentTF != RackPrivateTF) { tileTF.position = newParentTF.position; }
        tileTF.SetSiblingIndex(newSibIx);
        tileTF.GetChild(0).position = tileTF.position;
    }

    // overload without a sibling index. sends tile to last spot
    public void MoveTile(Transform newParent)
    {
        MoveTile(newParent, newParent.childCount);
    }

    public static void SetFaceBackToParent(PointerEventData eventData)
    {
        Transform faceTF = eventData.pointerDrag.transform;
        Transform tileTF = eventData.pointerDrag.GetComponent<TileLocomotion>().TileTF;
        faceTF.SetParent(tileTF);
        faceTF.position = tileTF.position;
    }
}
