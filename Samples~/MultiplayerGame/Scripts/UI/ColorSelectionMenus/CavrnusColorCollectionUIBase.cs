using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public abstract class CavrnusColorCollectionUIBase : MonoBehaviour
    {
        [SerializeField] protected string ContainerName;
        [SerializeField] protected string StringName;
        
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform container;
        [SerializeField] protected CavrnusColorCollection ColorCollection;

        private readonly List<ColorTextureChangerItem> items = new List<ColorTextureChangerItem>();

        protected CavrnusSpaceConnection SpaceConn;
        protected IDisposable Binding;

        protected virtual void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                sc.AwaitLocalUser(lu => {
                    SpaceConn = sc;
                    
                    PopulateItems();
                    BindProperty();
                });
            });
        }
        
        private void PopulateItems()
        {
            foreach (var data in ColorCollection.ColorData) {
                var go = Instantiate(prefab, container);
                items.Add(go.GetComponent<ColorTextureChangerItem>());
                go.GetComponent<ColorTextureChangerItem>().Setup(data, OnSelected);
            }
        }

        protected abstract void BindProperty();
        protected abstract void OnSelected(CavrnusColorCollection.ColorTextureInfo data);

        protected void SetSelectedItem(CavrnusColorCollection.ColorTextureInfo selectedData)
        {
            if (selectedData != null) {
                foreach (var item in items)
                    item.SetSelectionState(item.Info.DisplayName.ToLowerInvariant().Trim().Equals(selectedData.DisplayName.ToLowerInvariant().Trim()));
            }
        }

        private void OnDestroy()
        {
            Binding?.Dispose();
        }
    }
}