using System;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusSpaceInfoUIBindings : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI spaceName;
        private IDisposable binding;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                binding = sc.BindSpaceInfo(info => {
                    spaceName.text = $"Current Space: {info.Name}";
                });
            });
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}