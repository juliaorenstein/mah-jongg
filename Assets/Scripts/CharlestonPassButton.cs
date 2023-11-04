using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CharlestonPassButton : MonoBehaviour
    , IPointerClickHandler

{
    public ObjectReferences Refs;
    private GameManager GManager;
    private Charleston Charleston;
    private Button Button;
    private GameObject SkipButtonGO;
    private TextMeshProUGUI Text;

    private void Awake()
    {
        GManager = Refs.GameManager.GetComponent<GameManager>();
        Charleston = Refs.CharlestonBox.GetComponent<Charleston>();
        Button = GetComponent<Button>();
        SkipButtonGO = Refs.SkipCharlestonButton;
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GManager.Offline)
        {
            Charleston.C_StartPass();
        }
        else
        {
            Button.interactable = false;
            Text.SetText("Waiting for others");
            Charleston.C_StartPass();
        }
    }

    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay

        if (counter == -1 || counter == 7)
        {
            Button.gameObject.SetActive(false);
            Charleston.gameObject.SetActive(false);
            SkipButtonGO.SetActive(false);
            Refs.GameManager.GetComponent<TurnManager>().StartGamePlay();
            return;
        }

        // TODO: blind and optional passes
        // TODO: press space to pass

        // if it's a blind pass, allow pass whenever.
        // otherwise, make button not interactable.
        // if (!(counter == 2 || counter == 5))
        // { Button.interactable = false; }

        // commenting out blind pass logic here until
        // i build it out in HostPassLogic
        Button.interactable = false;

        // set the direction text
        Text.SetText($"Pass {Charleston.Direction()}");
    }


}
