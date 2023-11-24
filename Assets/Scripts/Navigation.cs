using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Navigation : MonoBehaviour
{
    public ObjectReferences Refs;
    private EventSystem ESystem;
    private Transform Rack;
    private Transform SelectedTF;
    private Transform Charleston;
    private CharlestonPassButton ChButton;
    private GameObject DiscardTF;
    private GameObject WaitButton;
    private GameObject PassButton;
    private GameObject CallButton;
    private NetworkCallbacks NCallbacks;

    private void Start()
    {
        ESystem = EventSystem.current;
        Rack = Refs.LocalRack.transform.GetChild(1);
        Charleston = Refs.Charleston;
        ChButton = Charleston.GetComponentInChildren<CharlestonPassButton>();
        DiscardTF = Refs.Discard;
        WaitButton = Refs.CallWaitButtons.transform.GetChild(0).gameObject;
        PassButton = Refs.CallWaitButtons.transform.GetChild(1).gameObject;
        CallButton = Refs.CallWaitButtons.transform.GetChild(2).gameObject;
        NCallbacks = Refs.NetworkCallbacks;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!SelectedTF)
            {
                Select(Rack.GetChild(0));
                return;
            }

            if (SelectedTF.IsChildOf(Rack))
            {
                int ix = SelectedTF.GetSiblingIndex() + 1;
                if (ix == Rack.childCount) { ix = 0; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { SelectedTF.SetSiblingIndex(ix); }
                else { Select(Rack.GetChild(ix)); }
                return;
            }

            if (SelectedTF.IsChildOf(Charleston))
            {
                Tile SelectedTile = SelectedTF.GetComponent<Tile>();
                Tile[] tilesInCharleston = Charleston.GetComponentsInChildren<Tile>();
                if (tilesInCharleston[^1] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx + 1].transform);
                }
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!SelectedTF)
            {
                Select(Rack.GetChild(0));
                return;
            }

            if (SelectedTF.IsChildOf(Rack))
            {
                int ix = SelectedTF.GetSiblingIndex() - 1;
                if (ix < 0) { ix = Rack.childCount - 1; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { SelectedTF.SetSiblingIndex(ix); }
                else { Select(Rack.GetChild(ix)); }
                return;
            }

            if (SelectedTF.IsChildOf(Charleston))
            {
                Tile SelectedTile = SelectedTF.GetComponent<Tile>();
                Tile[] tilesInCharleston = Charleston.GetComponentsInChildren<Tile>();
                if (tilesInCharleston[0] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx - 1].transform);
                }
                return;
            }
        }

        // TODO: shift + arrow to move tiles on rack
        // TODO: main gameplay support (not charleston)

        if (Input.GetKeyDown(KeyCode.DownArrow) && SelectedTF && SelectedTF.IsChildOf(Charleston))
        {
            Select(Rack.GetChild(0));
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && SelectedTF
            && SelectedTF.IsChildOf(Rack) && Charleston.gameObject.activeInHierarchy)
        {
            Tile charlestonTile = Charleston.GetComponentInChildren<Tile>();
            if (charlestonTile)
            {
                Select(charlestonTile.transform);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Charleston.gameObject.activeInHierarchy)
            {
                SelectedTF.GetComponentInChildren<TileLocomotion>().DoubleClickCharleston();
                return;
            }

            if (DiscardTF.GetComponentInChildren<Image>().raycastTarget)
            {
                SelectedTF.GetComponentInChildren<TileLocomotion>().DoubleClickDiscard();
                Unselect();
                return;
            }

            if (WaitButton.activeInHierarchy)
            {
                InputWait();
                return;
            }

            if (PassButton.activeInHierarchy)
            {
                InputPass();
                return;
            }

            if (!SelectedTF) {
                Select(Rack.GetChild(0));
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Charleston.gameObject.activeInHierarchy)
            {
                ChButton.InitiatePass();
                Unselect();
                return;
            }

            if (CallButton.activeInHierarchy)
            {
                InputCall();
                return;
            }
        }

        if (ESystem.currentSelectedGameObject == WaitButton)
        {
            InputWait();
            return;
        }

        if (ESystem.currentSelectedGameObject == PassButton)
        {
            InputPass();
            return;
        }

        if (ESystem.currentSelectedGameObject == CallButton)
        {
            InputCall();
            return;
        }

        // TODO: up/down between public/private rack
        // TODO: space bar to expose
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

    void InputWait()
    {
        NCallbacks.inputStruct.turnOptions.Set(TurnButtons.wait, true);
        WaitButton.SetActive(false);
        PassButton.SetActive(true);
        Unselect(); // in if statement to avoid unselecting unrelated things
    }

    void InputPass()
    {
        NCallbacks.inputStruct.turnOptions.Set(TurnButtons.pass, true);
        PassButton.SetActive(false);
        WaitButton.SetActive(true);
        CallButton.transform.parent.gameObject.SetActive(false);
        Unselect();
    }

    void InputCall()
    {
        NCallbacks.inputStruct.turnOptions.Set(TurnButtons.call, true);
        Unselect();
    }
}
