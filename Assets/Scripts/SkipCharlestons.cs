using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkipCharlestons : MonoBehaviour, IPointerClickHandler
{
    public CharlestonPassButton PassButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        PassButton.UpdateButton(-1);
        gameObject.SetActive(false);
    }
}
