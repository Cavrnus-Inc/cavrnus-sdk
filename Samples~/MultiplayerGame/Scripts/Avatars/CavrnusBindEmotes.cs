using System;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusBindEmotes : MonoBehaviour
    {
        [SerializeField] private bool isLocal;
        [SerializeField] private CavrnusAvatarAnimationController animController;
        private IDisposable binding;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                if (isLocal) {
                    sc.AwaitLocalUser(lu => {
                        lu.BindStringPropertyValue(CavrnusPropertyInfo.PlayerEmoteProperty, OnPropertyUpdated);
                    });
                }
                else {
                    gameObject.GetCavrnusUserFlagInParent(cf => {
                        binding = sc.BindStringPropertyValue(cf.User.ContainerId, CavrnusPropertyInfo.PlayerEmoteProperty, OnPropertyUpdated);
                    });
                }
            });
        }

        private void OnPropertyUpdated(string emote)
        {
            if (!string.IsNullOrWhiteSpace(emote))
                animController.TriggerEmote(emote);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}