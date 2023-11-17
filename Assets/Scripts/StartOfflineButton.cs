using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartOfflineButton : MonoBehaviour, IPointerClickHandler
{
    private NetworkCallbacks spawner;

    private void Awake()
    {
        spawner = GameObject.Find("NetworkRunner").GetComponent<NetworkCallbacks>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //GameObject Managers = Instantiate(Resources.Load<GameObject>("Prefabs/Managers"));
        //Managers.GetComponent<Setup>().O_Setup();

        spawner.StartGame(GameMode.Single);
        GetComponent<Button>().interactable = false;
        GetComponentInChildren<TextMeshProUGUI>().SetText("Connecting...");
    }
}