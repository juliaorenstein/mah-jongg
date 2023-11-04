using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;

public class DealClient : MonoBehaviour, IPointerClickHandler
{
    public ObjectReferences Refs;
    private PlayerRef Player;

    private void Awake()
    {
        Player = Refs.Runner
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
