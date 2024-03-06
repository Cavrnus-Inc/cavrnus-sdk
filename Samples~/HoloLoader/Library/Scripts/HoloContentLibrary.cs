using System;
using System.Collections.Generic;
using System.Linq;
using CavrnusSdk.API;
using CavrnusSdk.UI;
using Collab.Proxy.Comm.LiveTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusCore.Library
{
    [RequireComponent(typeof(Pagination))]
    public class HoloContentLibrary : MonoBehaviour
    {
        private class HoloLibraryOption : IListElement
        {
            public CavrnusRemoteContent Content;
            public event Action<CavrnusRemoteContent> Selected;
            public HoloLibraryOption(CavrnusRemoteContent content, Action<CavrnusRemoteContent> selected)
            {
                Content = content;
                Selected = selected;
            }
            
            public void EntryBuilt(GameObject element)
            {
                element.GetComponent<HoloLibraryItem>().Setup(Content, Selected);
            }
        }
        
        public Action<CavrnusRemoteContent> OnSelect;
        
        [SerializeField] private Button buttonVis;
        [SerializeField] private GameObject visibleIcon;
        [SerializeField] private GameObject hiddenIcon;
        [SerializeField] private GameObject mainContent;

        [SerializeField] private GameObject libraryItemPrefab;

        [Space]
        [SerializeField] private TMP_InputField searchField;

        [Space]
        [SerializeField] private GameObject resultsArea;
        [SerializeField] private TextMeshProUGUI resultsMessageText;

        private Pagination pagination;
        private List<CavrnusRemoteContent> allContent;

        private void Awake()
        {
            pagination = GetComponent<Pagination>();
            
            searchField.interactable = false;
            
            resultsArea.SetActive(false);
            visibleIcon.SetActive(false);
            hiddenIcon.SetActive(true);
            resultsMessageText.gameObject.SetActive(false);
            
            mainContent.SetActive(false);
            buttonVis.onClick.AddListener(() => {
                if (mainContent.activeSelf) {
                    visibleIcon.SetActive(false);
                    hiddenIcon.SetActive(true);
                }
                else {
                    visibleIcon.SetActive(true);
                    hiddenIcon.SetActive(false);
                }
                
                mainContent.SetActive(!mainContent.activeSelf);
            });
        }
        
        private void Start() 
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => 
            {
                CavrnusFunctionLibrary.FetchAllUploadedContent(content => {
                    allContent = content.Where(c => c.FileType == ObjectCategoryEnum.Holo).ToList();
                    if (allContent.Count == 0)
                    {
                        Debug.LogError($"No content found on initial load!");
                        return;
                    }
                    
                    searchField.interactable = true;
                    
                    searchField.onValueChanged.AddListener(DoSearch);
                });
            }); 
        }

        private void DoSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) {
                pagination.ResetPagination();
                
                resultsArea.SetActive(false);
                return;
            }
            
            var found = allContent.Where(c => c.Name.ToLowerInvariant().Contains(value.ToLowerInvariant())).ToList();
            if (found.Count == 0) {
                resultsMessageText.gameObject.SetActive(true);
                resultsArea.SetActive(false);

                pagination.ResetPagination();
                
                return;
            }

            resultsMessageText.gameObject.SetActive(false);
            resultsArea.SetActive(true);

            var options = new List<IListElement>();
            found.ForEach(f => options.Add(new HoloLibraryOption(f,Selected)));
            
            pagination.NewPagination(libraryItemPrefab, options);
        }

        private void Selected(CavrnusRemoteContent obj)
        {
            OnSelect?.Invoke(obj);
            Debug.Log($"Selected {obj.Name}");
        }
    }
}