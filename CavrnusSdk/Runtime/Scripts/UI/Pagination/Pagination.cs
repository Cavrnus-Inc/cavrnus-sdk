using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class Pagination : MonoBehaviour
    {
        [SerializeField] private int itemsPerPage = 10;
        
        [Space]
        [SerializeField] private Button buttonPrev;
        [SerializeField] private Button buttonNext;

        [Space]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform parentContainer;

        [Space]
        [SerializeField] private TextMeshProUGUI currentPageText;
        
        private GameObject itemPrefab;

        private int currentPage;
        private int totalPages;

        private List<IListElement> content;

        private void Awake()
        {
            ResetPagination();
            
            buttonPrev.onClick.AddListener(Prev);
            buttonNext.onClick.AddListener(Next);
        }

        public void NewPagination(GameObject itemPrefab, List<IListElement> content)
        {
            this.itemPrefab = itemPrefab;
            this.content = content;
            
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

        private Dictionary<GameObject, IListElement> currentItems = new Dictionary<GameObject, IListElement>();
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
                    Destroy(go.Key);

                currentItems.Clear();
            }

            var start = (page - 1) * itemsPerPage;
            var end = Mathf.Min(start + itemsPerPage, content.Count);
            
            for (var i = start; i < end; i++)   
            {
                var newItem = Instantiate(itemPrefab, itemContainer);
                currentItems.Add(newItem.gameObject, content[i]);
                currentItems[newItem.gameObject].EntryBuilt(newItem);
            }

            parentContainer.gameObject.SetActive(true);
            currentPageText.text = $"{page} of {totalPages}";
            currentPage = page;
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
                    Destroy(go.Key);

                currentItems.Clear();
            }
            
            parentContainer.gameObject.SetActive(false);
        }
    }
}