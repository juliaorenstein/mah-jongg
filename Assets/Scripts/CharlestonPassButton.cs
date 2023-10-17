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
    private TextMeshProUGUI Text;

    private void Awake()
    {
        GManager = Refs.GameManager.GetComponent<GameManager>();
        Charleston = Refs.CharlestonBox.GetComponent<Charleston>();
        Button = GetComponent<Button>();
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GManager.Offline)
        {
            Charleston.StartPassFromLocal();
        }
        else
        {
            Button.interactable = false;
            Text.SetText("Waiting for others");
            Charleston.StartPassFromLocal();
        }
    }

    public void UpdateButton(int counter)
    {
        // if Counter is -1, remove the button and start main gameplay
        if (counter == -1)
        {
            Button.gameObject.SetActive(false);
            Charleston.gameObject.SetActive(false);
            Refs.GameManager.GetComponent<TurnManager>().FirstTurn();
            return;
        }

        // if it's a blind pass, allow pass whenever.
        // otherwise, make button not interactable.
        // if (!(counter == 2 || counter == 5))
        // { Button.interactable = false; }

        // commenting out blind pass logic here until
        // i build it out in HostPassLogic
        Button.interactable = false;

        // set the direction text
        Text.SetText($"Pass {Charleston.Direction}");
    }

    
}
