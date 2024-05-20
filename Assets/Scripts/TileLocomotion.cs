using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileLocomotion : MonoBehaviour
    , IPointerClickHandler
    , ISelectHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    private ObjectReferences Refs;
    //private GameManager GManager;
    private CharlestonManager CManager;
    private EventSystem ESystem;
    public Transform TileTF;
    private Transform RackPrivateTF;
    private Transform RackPublicTF;
    private Transform OtherRacksTF;
    private Image TileImage;
    private Transform DraggingTF;
    private Transform CharlestonBoxTF;
    private Transform DiscardTF;
    private TurnManager TManager;
    private int TileID;
    private List<Transform> RebuildLayoutTransforms;

    // lerp stuff
    bool Lerping = false;
    Vector3 StartPos;
    Vector3 EndPos;
    readonly float TotalLerpTime = 0.2f;
    float CurrentLerpTime = 0.2f;

    private void Awake()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        ESystem = Refs.EventSystem.GetComponent<EventSystem>();
        TileTF = transform.parent;
        RackPrivateTF = Refs.LocalRack.transform.GetChild(1);
        RackPublicTF = Refs.LocalRack.transform.GetChild(0);
        OtherRacksTF = Refs.OtherRacks.transform;
        TileImage = GetComponent<Image>();
        DraggingTF = Refs.Dragging.transform;
        CharlestonBoxTF = Refs.Charleston.GetChild(0);
        DiscardTF = Refs.Discard.transform;
        RebuildLayoutTransforms = new() { DiscardTF, RackPrivateTF, RackPublicTF };
    }

    private void Start()
    {
        //GManager = Refs.Managers.GetComponent<GameManager>();
        CManager = Refs.Managers.GetComponent<CharlestonManager>();
        TManager = Refs.Managers.GetComponent<TurnManager>();
        TileID = GetComponentInParent<TileComponent>().tile.ID;
        foreach ( Transform rack in OtherRacksTF )
        {
            RebuildLayoutTransforms.Add(rack.GetChild(0));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            if (CharlestonBoxTF.parent.gameObject.activeSelf)
            { DoubleClickCharleston(); }
            else if (EligibleForExpose())
            { DoubleClickExpose(); }
            else if (DiscardTF.GetComponent<Image>().raycastTarget)
            { DoubleClickDiscard(); }
        }
    }

    public void DoubleClickCharleston()
    {
        if (transform.IsChildOf(RackPrivateTF))
        {
            for (int i = 0; i < 3; i++)
            {
                Transform chSpot = CharlestonBoxTF.GetChild(i);
                if (chSpot.childCount == 0)
                {
                    MoveTile(chSpot);
                    break;
                }
            }
            if (transform.IsChildOf(RackPrivateTF))
            {
                CharlestonBoxTF.GetChild(2)
                               .GetComponentInChildren<TileLocomotion>()
                               .MoveTile(RackPrivateTF);
                MoveTile(CharlestonBoxTF.GetChild(2));
            }
        }
        else { MoveTile(RackPrivateTF); }
        CManager.C_CheckDone();
    }

    public void DoubleClickExpose()
    { TManager.C_Expose(TileID); }

    public void DoubleClickDiscard()
    {
        TManager.C_Discard(TileID);
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
        IEnumerable<Transform> raycastTFs
            = raycastResults.Select(res => res.gameObject.transform);
        float dropXPos = eventData.pointerDrag.transform.position.x;

        // drop on rack
        if (raycastTFs.Contains(RackPrivateTF)) { DropOnRack(dropXPos); }

        // drop on charleston
        else if (raycastTFs.Contains(CharlestonBoxTF)
            && eventData.pointerCurrentRaycast.gameObject != CharlestonBoxTF.gameObject)
        { DropOnCharleston(eventData); }

        // discard
        else if (raycastTFs.Contains(DiscardTF))
        { TManager.C_Discard(TileID); }

        // expose
        else if (raycastTFs.Contains(RackPrivateTF.parent.GetChild(0).transform)
            && EligibleForExpose())
        { TManager.C_Expose(TileID); }

        // otherwise, move the tile back to where it came from
        else { MoveBack(); }
        CManager.C_CheckDone();
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

    public bool EligibleForExpose()
    {
        return transform.IsChildOf(RackPrivateTF)
            && (TManager.ExposeTileName == transform.parent.name
            || (TManager.ExposeTileName != null && transform.parent.name == "Joker"));
    }

    void MoveBack()
    {
        MoveTile(TileTF.parent, TileTF.GetSiblingIndex());
    }

    public void MoveTile(Transform newParentTF, int newSibIx)
    {
        if (transform.IsChildOf(Refs.TilePool.transform))
        {
            StartPos = new Vector3(100, 100, 100);
        }
        else { StartPos = transform.position; }
        EndPos = newParentTF.position;

        TileTF.SetParent(newParentTF);
        TileTF.SetSiblingIndex(newSibIx);
        TileTF.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 0);

        if (RebuildLayoutTransforms.Contains(newParentTF))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)newParentTF);
            EndPos = TileTF.position;
        }

        else
        {
            transform.parent.position = newParentTF.position;
        }

        Lerping = true;
        
        // TODO: add racklist management?
        // FIXME: newly dealt tiles should lerp from somewhere
        // FIXME: when calling a tile it flashes briefly on rack before lerping
    }

    // overload without a sibling index. sends tile to last spot
    public void MoveTile(Transform newParent)
    {
        MoveTile(newParent, newParent.childCount);
    }

    public static void MoveTile(Transform tileTF, Transform destination)
    {
        tileTF.GetComponentInChildren<TileLocomotion>().MoveTile(destination);
    }

    public static void MoveTile(int tileID, Transform destination)
    {
        MoveTile(GameManager.TileList[tileID].transform, destination);
    }
    // FIXME: you can discard multiple tiles during discard 2 sec

    private void Update()
    {
        if (Lerping)
        {
            float t = CurrentLerpTime / TotalLerpTime;
            transform.position = Vector3.Lerp(StartPos, EndPos, t);

            CurrentLerpTime += Time.deltaTime;
            if (CurrentLerpTime > TotalLerpTime)
            {
                transform.position = transform.parent.position; // not EndPos because of weird rack shift bug
                CurrentLerpTime = 0f;
                Lerping = false;
            }
        }
        
    }
}
