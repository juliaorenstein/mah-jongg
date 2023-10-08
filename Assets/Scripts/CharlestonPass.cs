using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CharlestonPass : MonoBehaviour
    , IPointerClickHandler

{
    public ObjectReferences Refs;
    private GameManager GManager;
    private Charleston Charleston;

    private void Awake()
    {
        GManager = Refs.GameManager.GetComponent<GameManager>();
        Charleston = Refs.CharlestonBox.GetComponent<Charleston>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GManager.Offline)
        {

        }
        else
        {
            GetComponent<Button>().interactable = false;
            GetComponentInChildren<TextMeshProUGUI>()
                     .SetText("Waiting for others");
            Charleston.Pass();
        }
    }

    

    
}
