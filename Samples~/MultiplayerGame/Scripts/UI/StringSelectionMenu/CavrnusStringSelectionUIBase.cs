using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public abstract class CavrnusStringSelectionUIBase : MonoBehaviour
    {
        [Space]
        [SerializeField] protected CavrnusStringSelectionItem Prefab;
        [SerializeField] protected Transform Container;

        protected readonly List<CavrnusStringSelectionItem> Items = new List<CavrnusStringSelectionItem>();

        protected CavrnusSpaceConnection SpaceConn;
        protected CavrnusUser LocalUser;
        private IDisposable binding;

        protected virtual void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                sc.AwaitLocalUser(lu => {
                    SpaceConn = sc;
                    LocalUser = lu;
                    BindProperty();
                    PopulateItems();
                });
            });
        }

        protected abstract void PopulateItems();
        protected abstract void BindProperty();
        protected abstract void PostOrSendValue(string val);

        protected void OnServerValueUpdated(string severVal)
        {
            SetSelectedItem(severVal);
        }

        protected void SetSelectedItem(string selectedVal)
        {
            if (!string.IsNullOrWhiteSpace(selectedVal)) {
                foreach (var item in Items)
                    item.SetSelectionState(item.StringValue.ToLowerInvariant().Trim().Equals(selectedVal.ToLowerInvariant().Trim()));
            }
        }

        protected virtual void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}