using System;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame.Environment
{
    public class CavrnusSyncCameraColor : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        
        private IDisposable binding;

        private void Start()
        {
            if (cam == null)
                cam = Camera.main;
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                binding = sc.BindColorPropertyValue(CavrnusPropertyInfo.EnvironmentContainer, CavrnusPropertyInfo.EnvironmentColor, OnPropertyUpdated);
            });
        }

        private void OnPropertyUpdated(Color color)
        {
            cam.backgroundColor = color;
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}