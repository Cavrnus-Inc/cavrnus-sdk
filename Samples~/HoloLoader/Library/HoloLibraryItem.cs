using System;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoloLibraryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private GameObject downloadButton;
    
    private CavrnusRemoteContent content;
    private Action<CavrnusRemoteContent> onSelected;

    public void Setup(CavrnusRemoteContent content, Action<CavrnusRemoteContent> onSelected)
    {
        this.content = content;
        this.onSelected = onSelected;

        itemName.text = content.FileName;
        
        downloadButton.gameObject.SetActive(false);
    }

    public void Select() => onSelected?.Invoke(content);

    public void OnPointerEnter(PointerEventData eventData)
    {
        downloadButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        downloadButton.gameObject.SetActive(false);
    }
}
