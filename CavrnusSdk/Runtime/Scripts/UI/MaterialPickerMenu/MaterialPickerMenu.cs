using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.Events;

namespace CavrnusSdk.CollaborationExamples
{
    public class MaterialPickerMenu : MonoBehaviour
    {
        public UnityEvent<string> OnMaterialSelected;
        
        [SerializeField] private string containerName;
        [SerializeField] private string propertyName;
        
        [Space]
        [SerializeField] private List<Material> materials;

        [Space] 
        [SerializeField] private MaterialPickerEntry materialPickerEntryPrefab;
        [SerializeField] private Transform entriesContainer;

        private readonly List<MaterialPickerEntry> materialEntries = new List<MaterialPickerEntry>();

        private IDisposable binding;
        private CavrnusSpaceConnection spaceConnection;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConnection = spaceConn;
                
                foreach (var material in materials) {
                    var item = Instantiate(materialPickerEntryPrefab, entriesContainer);
                    item.Setup(material, MaterialSelected);
                    
                    materialEntries.Add(item);
                }
                
                binding = spaceConnection.BindStringPropertyValue(containerName, propertyName, val => {
                    OnMaterialSelected?.Invoke(val);

                    foreach (var item in materialEntries) {
                        item.SetSelectionState(string.Equals(item.MaterialName, val));
                    }
                });
            });
        }

        private void MaterialSelected(Material material)
        {
            spaceConnection?.PostStringPropertyUpdate(containerName, propertyName,material.name);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}