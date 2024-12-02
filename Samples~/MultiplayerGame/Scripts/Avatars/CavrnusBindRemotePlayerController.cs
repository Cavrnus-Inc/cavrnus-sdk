using System;
using CavrnusSdk.API;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusBindRemotePlayerController : MonoBehaviour
    {
        [SerializeField] private CavrnusAvatarInputReceiver inputReceiver;

        private IDisposable binding;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                gameObject.GetCavrnusUserFlagInParent(cf => {
                    binding = sc.BindVectorPropertyValue(cf.User.ContainerId, CavrnusPropertyInfo.PlayerInputProperty, OnPropertyUpdated);
                });
            });
        }
        
        private void OnPropertyUpdated(Vector4 input)
        {
            inputReceiver.HandleMovementInput(input.ToFloat3().ToVec3());
        }
        
        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}