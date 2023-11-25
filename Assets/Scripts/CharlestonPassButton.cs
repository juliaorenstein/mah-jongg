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
    public GameManager GManager;
    public CharlestonManager CManager;
    private GameObject Charleston;
    private Button Button;
    private GameObject SkipButtonGO;
    private TextMeshProUGUI Text;

    private void Awake()
    {
        Refs = GameObject.Find("ObjectReferences").GetComponent<ObjectReferences>();
        // FIXME: don't call find
        Button = GetComponent<Button>();
        SkipButtonGO = transform.parent.GetChild(2).gameObject;
        Text = GetComponentInChildren<TextMeshProUGUI>();
        Charleston = transform.parent.gameObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    { InitiatePass(); }

    public void InitiatePass()
    {
        if (Button.interactable)
        {
            Button.interactable = false;
            Text.SetText("Waiting for others");
            CManager.C_StartPass();
        }      
    }

    public void UpdateButton() { UpdateButton(CManager.Counter); }
    
    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay
        if (counter == -1 || counter == 7)
        {
            Charleston.SetActive(false);
            Refs.Managers.GetComponent<TurnManager>().C_StartGamePlay();
            return;
        }

        Button.interactable = false;

        // set the direction text
        Text.SetText($"Pass {CManager.Direction(counter)}");
    }

    public void NoJokers()
    {
        Text.SetText("You can't pass jokers");
    }
}
