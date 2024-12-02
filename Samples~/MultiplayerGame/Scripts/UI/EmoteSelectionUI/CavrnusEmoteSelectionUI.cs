using CavrnusSdk.API;
using CavrnusSdk.MultiplayerGame;
using UnityEngine;

namespace CavrnusDemo.EmoteSelectionUI
{
    public class CavrnusEmoteSelectionUI : CavrnusStringSelectionUIBase
    {
        [SerializeField] private CavrnusEmoteCollection emotes;
        
        private CavrnusLivePropertyUpdate<string> updater;

        protected override void PopulateItems()
        {
            foreach (var at in emotes.EmoteData) {
                var go = Instantiate(Prefab, Container);
                Items.Add(go.GetComponent<CavrnusStringSelectionItem>());
                go.Setup(at.DisplayName, PostOrSendValue);
            }
        }

        protected override void BindProperty()
        {
            updater = null;
            LocalUser.BindStringPropertyValue(CavrnusPropertyInfo.PlayerEmoteProperty, OnServerValueUpdated);
        }

        protected override void PostOrSendValue(string val)
        {
            updater ??= SpaceConn.BeginTransientStringPropertyUpdate(LocalUser.ContainerId, CavrnusPropertyInfo.PlayerEmoteProperty, val);
            updater?.UpdateWithNewData(val);
            updater?.Cancel();
            updater = null;
        }
    }
}