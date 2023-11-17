using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartNetworkButton : MonoBehaviour, IPointerClickHandler
{
    private NetworkCallbacks spawner;

    private void Awake()
    {
        spawner = GameObject.Find("NetworkRunner").GetComponent<NetworkCallbacks>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        spawner.StartGame(GameMode.AutoHostOrClient);
        GetComponent<Button>().interactable = false;
        GetComponentInChildren<TextMeshProUGUI>().SetText("Connecting...");
    }
}
