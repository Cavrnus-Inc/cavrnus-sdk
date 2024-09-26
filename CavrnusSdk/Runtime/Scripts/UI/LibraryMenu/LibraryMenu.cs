using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    [RequireComponent(typeof(Pagination))]
    public class LibraryMenu : MonoBehaviour
    {
        private class LibraryOption : IListElement
        {
            public CavrnusRemoteContent Content;
            public event Action<CavrnusRemoteContent> Selected;
            public LibraryOption(CavrnusRemoteContent content, Action<CavrnusRemoteContent> selected)
            {
                Content = content;
                Selected = selected;
            }
            
            public void EntryBuilt(GameObject element)
            {
                element.GetComponent<LibraryMenuItem>().Setup(Content, Selected);
            }
        }
        
        public Action<CavrnusRemoteContent> OnSelect;
        
        [SerializeField] private Button buttonVis;
        [SerializeField] private GameObject visibleIcon;
        [SerializeField] private GameObject hiddenIcon;
        [SerializeField] private GameObject mainContent;

        [SerializeField] private GameObject libraryItemPrefab;

        [Space]
        [SerializeField] private Button buttonUpload;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_InputField uploadFilePath;
        [SerializeField] private TMP_Dropdown filterDropdown;

        private Pagination pagination;
        private List<CavrnusRemoteContent> allContent;
        private List<CavrnusRemoteContent> filteredContent;

        private void Awake()
        {
            pagination = GetComponent<Pagination>();
            
            searchField.interactable = false;
            
            visibleIcon.SetActive(false);
            hiddenIcon.SetActive(true);
            
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
        
        private TMP_Dropdown.OptionData allFilterOption;
        private void Start() 
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => 
            {
                allFilterOption = new TMP_Dropdown.OptionData("All");
                searchField.interactable = true;
                searchField.onValueChanged.AddListener(DoSearch);
                
                filterDropdown.onValueChanged.AddListener(FilterUpdated);
                uploadFilePath.onValueChanged.AddListener(FilePathInputFieldChanged);
                
                buttonUpload.onClick.AddListener(DoUpload);
                buttonUpload.interactable = false;

                RefreshContent();
            }); 
        }

        private void FilePathInputFieldChanged(string val)
        {
            buttonUpload.interactable = !String.IsNullOrWhiteSpace(val);
        }
        
        private TMP_Dropdown.OptionData currFilterOption;
        private void FilterUpdated(int selectedIndex)
        {
            currFilterOption = filterDropdown.options[selectedIndex];
            if (currFilterOption.text == "All") {
                filteredContent = allContent;
            }
            else {
                filteredContent = allContent
                                  .Where(c => Path.GetExtension(c.FileName).Equals(currFilterOption.text, StringComparison.OrdinalIgnoreCase))
                                  .ToList();
            }
            
            UpdatePagination(filteredContent);
        }

        public void RefreshContent()
        {
            CavrnusFunctionLibrary.FetchAllUploadedContent(content => {
                allContent = content;
                if (content.Count == 0) {
                    Debug.LogError("No content found on load!");
                    return;
                }

                PopulateDropdownFilter(content);

                // Reset filter too?
                FilterUpdated(0);
            });
        }
        
        private void PopulateDropdownFilter(IEnumerable<CavrnusRemoteContent> crc)
        {
            var foundExtensions = crc
                                  .Select(content => Path.GetExtension(content.FileName).ToLower())
                                  .Distinct();
            
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(allFilterOption);
            foreach (var fileType in foundExtensions) {
                options.Add(new TMP_Dropdown.OptionData(fileType));
            }
            
            filterDropdown.ClearOptions();
            filterDropdown.AddOptions(options);
        }
        
        public void DoUpload()
        {
            CavrnusFunctionLibrary.UploadContent(uploadFilePath.text, crc => {
                uploadFilePath.text = "";

                Debug.Log($"{uploadFilePath.text} successfully uploaded! ContentId: {crc.Id}");
            });
        }

        private void DoSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) {
                pagination.ResetPagination();
                
                UpdatePagination(filteredContent);
                
                return;
            }
            
            var found = filteredContent.Where(c => c.Name.ToLowerInvariant().Contains(value.ToLowerInvariant())).ToList();
            UpdatePagination(found);
        }
        
        private void UpdatePagination(List<CavrnusRemoteContent> content)
        {
            var options = new List<IListElement>();
            content.Sort((x, y) => String.Compare(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant(), StringComparison.Ordinal));
            content.ForEach(s => options.Add(new LibraryOption(s, Selected)));

            pagination.NewPagination(libraryItemPrefab, options);
        }

        private void Selected(CavrnusRemoteContent crc)
        {
            OnSelect?.Invoke(crc);
            Debug.Log($"Selected {crc.Name}");
        }

        private void OnDestroy()
        {
            searchField.onValueChanged.RemoveListener(DoSearch);
            filterDropdown.onValueChanged.RemoveListener(FilterUpdated);
            buttonUpload.onClick.RemoveListener(DoUpload);
        }
    }
}