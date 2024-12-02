using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusAvatarTypeSelectionUI : CavrnusStringSelectionUIBase
    {
        [SerializeField] private CavrnusAvatarTypes avatars;
        
        protected override void PopulateItems()
        {
            foreach (var at in avatars.AvatarTypes) {
                var go = Instantiate(Prefab, Container);
                Items.Add(go.GetComponent<CavrnusStringSelectionItem>());
                go.Setup(at.Name, PostOrSendValue);
            }
        }

        protected override void BindProperty()
        {
            LocalUser.BindToUserMetadata(CavrnusPropertyInfo.PlayerAvatarProperty, OnServerValueUpdated);
        }

        protected override void PostOrSendValue(string val)
        {
            CavrnusFunctionLibrary.UpdateLocalUserMetadataString(CavrnusPropertyInfo.PlayerAvatarProperty, val);
        }
    }
}