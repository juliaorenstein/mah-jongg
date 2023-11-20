using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Navigation : MonoBehaviour
{
    public ObjectReferences Refs;
    private EventSystem ESystem;
    private Transform Rack;
    private Transform SelectedTF;
    private Transform Charleston;
    private CharlestonPassButton ChButton;

    private void Start()
    {
        ESystem = EventSystem.current;
        Rack = Refs.LocalRack.transform.GetChild(1);
        Charleston = Refs.Charleston;
        ChButton = Charleston.GetComponentInChildren<CharlestonPassButton>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!SelectedTF)
            {
                Select(Rack.GetChild(0));
            }
            else if (SelectedTF.IsChildOf(Rack))
            {
                int ix = SelectedTF.GetSiblingIndex() + 1;
                if (ix == Rack.childCount) { ix = 0; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { SelectedTF.SetSiblingIndex(ix); }
                else { Select(Rack.GetChild(ix)); }
            }
            else if (SelectedTF.IsChildOf(Charleston))
            {
                Tile SelectedTile = SelectedTF.GetComponent<Tile>();
                Tile[] tilesInCharleston = Charleston.GetComponentsInChildren<Tile>();
                if (tilesInCharleston[^1] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx + 1].transform);
                }  
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!SelectedTF)
            {
                Select(Rack.GetChild(0));
            }
            else if (SelectedTF.IsChildOf(Rack))
            {
                int ix = SelectedTF.GetSiblingIndex() - 1;
                if (ix < 0) { ix = Rack.childCount - 1; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { SelectedTF.SetSiblingIndex(ix); }
                else { Select(Rack.GetChild(ix)); }
            }
            else if (SelectedTF.IsChildOf(Charleston))
            {
                Tile SelectedTile = SelectedTF.GetComponent<Tile>();
                Tile[] tilesInCharleston = Charleston.GetComponentsInChildren<Tile>();
                if (tilesInCharleston[0] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx - 1].transform);
                }
            }
        }

        // TODO: shift + arrow to move tiles on rack
        // TODO: main gameplay support (not charleston)

        if (Input.GetKeyDown(KeyCode.DownArrow) && SelectedTF && SelectedTF.IsChildOf(Charleston))
        {
            Select(Rack.GetChild(0));
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && SelectedTF && SelectedTF.IsChildOf(Rack))
        {
            Tile charlestonTile = Charleston.GetComponentInChildren<Tile>();
            if (charlestonTile)
            {
                Select(charlestonTile.transform);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (SelectedTF && Charleston.gameObject.activeInHierarchy)
            {
                SelectedTF.GetComponentInChildren<TileLocomotion>().DoubleClickCharleston();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Unselect();
            ChButton.InitiatePass();
        }
    }

    public void Select(Transform tileTF)
    {
        if (SelectedTF)
        {
            SelectedTF.GetChild(0).GetChild(0).gameObject.SetActive(false); // unhighlight previous
        }
        ESystem.SetSelectedGameObject(tileTF.gameObject);
        tileTF.GetChild(0).GetChild(0).gameObject.SetActive(true); // highlight current
        SelectedTF = tileTF;
    }

    public void Unselect()
    {
        if (SelectedTF)
        {
            SelectedTF.GetChild(0).GetChild(0).gameObject.SetActive(false); // unhighlight previous
        }
        ESystem.SetSelectedGameObject(null);
        SelectedTF = null;
    }
}
