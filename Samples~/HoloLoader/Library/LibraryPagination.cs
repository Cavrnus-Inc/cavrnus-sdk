using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusCore.Library
{
    public class LibraryPagination : MonoBehaviour
    {
        [SerializeField] private int itemsPerPage = 10;
        
        [Space]
        [SerializeField] private Button buttonPrev;
        [SerializeField] private Button buttonNext;

        [Space]
        [SerializeField] private HoloLibraryItem itemPrefab;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform parentContainer;

        [Space]
        [SerializeField] private TextMeshProUGUI currentPageText;

        private int currentPage;
        private int totalPages;

        private List<CavrnusRemoteContent> content;

        private Action<CavrnusRemoteContent> selected;

        private void Awake()
        {
            ResetPagination();
        }

        private void Start()
        { 
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                buttonPrev.onClick.AddListener(Prev);
                buttonNext.onClick.AddListener(Next);
            });
        }

        public void NewPagination(List<CavrnusRemoteContent> content, Action<CavrnusRemoteContent> selected)
        {
            this.content = content;
            this.selected = selected;
            
            buttonPrev.interactable = false;
            buttonNext.interactable = true;
            
            currentPage = 1; // 1-indexed page
            totalPages = content.Count / itemsPerPage;
            
            // Account for any extra items
            if (content.Count % itemsPerPage != 0)
                totalPages++;
            
            LoadPage(currentPage);
        }

        private void Next()
        {
            buttonPrev.interactable = true;
            LoadPage(currentPage + 1);
        }

        private void Prev()
        {
            buttonNext.interactable = true;
            LoadPage(currentPage - 1);
        }

        private List<GameObject> currentItems = new List<GameObject>();
        private void LoadPage(int page)
        {
            if (page == 1) {
                buttonPrev.interactable = false;
            }
            
            // Arrive at last page
            if (page == totalPages) {
                buttonNext.interactable = false;
            }
            
            if (currentItems.Count > 0) {
                foreach (var go in currentItems)
                    Destroy(go);

                currentItems.Clear();
            }

            var start = (page - 1) * itemsPerPage;
            var end = Mathf.Min(start + itemsPerPage, content.Count);
            
            for (var i = start; i < end; i++)   
            {
                var newItem = Instantiate(itemPrefab, itemContainer);
                newItem.Setup(content[i], ItemSelected);
                
                currentItems.Add(newItem.gameObject);
            }

            parentContainer.gameObject.SetActive(true);
            currentPageText.text = $"{page} of {totalPages}";
            currentPage = page;
        }

        private void ItemSelected(CavrnusRemoteContent crc)
        {
            selected?.Invoke(crc);
        }
        
        private void OnDestroy()
        {
            buttonPrev.onClick.RemoveListener(Prev);
            buttonNext.onClick.RemoveListener(Next);
        }

        public void ResetPagination()
        {
            if (currentItems.Count > 0) {
                foreach (var go in currentItems)
                    Destroy(go);

                currentItems.Clear();
            }
            
            parentContainer.gameObject.SetActive(false);
        }
    }
}