using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragTest : MonoBehaviour
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(transform.parent.parent.GetChild(1));
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponentInParent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        GetComponentInParent<HorizontalLayoutGroup>().SetLayoutVertical();
    }
}
