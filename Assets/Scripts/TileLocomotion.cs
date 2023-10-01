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
        int TileTFIdx = TManager.LocalTiles.IndexOf(TileTF.gameObject);
        int overTileIdx = TManager.LocalTiles.IndexOf(overTile);

        // IF TO THE LEFT OF ORIGINAL POSITION, LEFT SIDE OF TILES RULE
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

        // IF POSITION IS ON RACK, DROP THE TILE AT NEW POSITION
        if (raycastGameObjects.Contains(RackPrivateTF.gameObject))
        {
            float thisDist;
            float prevDist = float.MaxValue;
            int closestTileIndex = RackPrivateTF.childCount - 1;
            Transform closestTileTF;
            int newIndex;

            // SUBTRACT CURRENT POSITION FROM THE POSITION OF EACH TILE STARTING
            // FROM THE LEFT. THIS VALUE WILL DECREASE AND THEN START INCREASING.
            // WHEN IT STARTS INCREASING, THEN THE PREVIOUS TILE WAS THE CLOSEST
            // ONE. SAVE OFF THAT INDEX TO closestTileIndex.
            // IF THE INCREASE IS NEVER REACHED, THEN THE TILE SHOULD GO TO THE
            // END OF THE RACK, WHICH IS WHY closestTileIndex IS INITIALIZED THERE.
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

            // NOW CHECK IF WE'RE TO THE LEFT OR RIGHT OF THE TILE AND SET ACCORDINGLY.
            closestTileTF = RackPrivateTF.GetChild(closestTileIndex);
            if (dropXPos > closestTileTF.position.x)
            {
                newIndex = closestTileIndex + 1;
            }
            else
            {
                newIndex = closestTileIndex;
            }
            // OFFSET ONE IF WE'RE MOVING TO THE RIGHT
            if (dropXPos > TileTF.transform.position.x)
            {
                newIndex--;
            }

            TileTF.SetSiblingIndex(newIndex);

            /*
             * // DETERMINE WHICH TILE WE'RE CLOSEST 
            // FIRST CREATE A LIST OF X POSITIONS OF TILES
            List<float> tileXPosList = RackPrivate
                .transform
                .GetComponentsInChildren<Transform>()
                .Select(tf => tf.position.x)
                .Distinct().ToList();

            // AND GET CURRENT X POS TO COMPARE TO
            float dropXPos = eventData.position.x;

            // GET THE MINIMUM DISTANCE
            float closestXPos = new List<float>(tileXPosList)
                .OrderBy(xPos => Math.Abs(dropXPos - xPos)).First();

            // AND THEN USE THAT VALUE TO FIND THE CLOSEST TILE
            Transform closestTile = RackPrivate.
                transform.GetChild(tileXPosList.IndexOf(closestXPos));
            

            // IF dropXPos > closestXPos THEN WE'RE TO THE RIGHT AND VICE VERSA
            // TO SET SIBLING INDEX ACCORDINGLY:
            // IF TO THE RIGHT, SET TO closestTile INDEX + 1
            // ELSE, SET TO closestTile INDEX
            int newIndex;

            if (dropXPos > closestXPos) newIndex = closestTile.GetSiblingIndex() + 1;
            else newIndex = closestTile.GetSiblingIndex();

            TileTF.SetSiblingIndex(newIndex);

            */
        }

        // TileTF.SetSiblingIndex(TileTF.parent.childCount - 1);
        transform.position = TileTF.position;
        HLG.SetLayoutHorizontal();
    }
}
