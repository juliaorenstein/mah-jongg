using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

public class DealClient : MonoBehaviour
    , IPointerClickHandler
{
    public ObjectReferences Refs;
    private Setup Setup;
    public PlayerRef Player;

    private void Awake()
    {
        Setup = Refs.Managers.GetComponent<Setup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        Setup.H_Setup(Player);
    }
}
