using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour, IPointerClickHandler
{
    private Spawner spawner;

    private void Awake()
    {
        spawner = GameObject.Find("NetworkRunner").GetComponent<Spawner>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        spawner.StartGame(GameMode.AutoHostOrClient);
        GetComponent<Button>().interactable = false;
        GetComponentInChildren<TextMeshProUGUI>().SetText("Connecting...");
    }
}
