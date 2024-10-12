using System;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CavrnusSdk.UI
{
    public class LibraryMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private GameObject downloadButtonIcon;
        
        private CavrnusRemoteContent content;
        private Action<CavrnusRemoteContent> onSelected;

        public void Setup(CavrnusRemoteContent content, Action<CavrnusRemoteContent> onSelected)
        {
            this.content = content;
            this.onSelected = onSelected;

            itemName.text = content.FileName;
            
            downloadButtonIcon.gameObject.SetActive(false);
        }

        public void Select() => onSelected?.Invoke(content);

        public void OnPointerEnter(PointerEventData eventData)
        {
            downloadButtonIcon.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            downloadButtonIcon.SetActive(false);
        }
    }
}