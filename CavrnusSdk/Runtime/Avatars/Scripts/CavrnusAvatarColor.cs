using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using CavrnusSdk.Setup;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk.Avatars
{
    public class CavrnusAvatarColor : MonoBehaviour
    {
        [SerializeField] private List<Renderer> primaryMeshRenderers;
        [SerializeField] private List<Renderer> secondaryMeshRenderers;
        
        private List<IDisposable> bindings = new List<IDisposable>();

        private void Start()
        {
            var user = gameObject.GetComponentInAllParents<CavrnusUserFlag>().User;
            if (user != null) {
                bindings.Add(user.BindColorPropertyValue("primaryColor", OnPrimaryColorUpdated));
                bindings.Add(user.BindColorPropertyValue("secondaryColor", OnSecondaryColorUpdated));
            }
        }

        private void OnPrimaryColorUpdated(Color val) => UpdateColor(primaryMeshRenderers, val);
        private void OnSecondaryColorUpdated(Color val) => UpdateColor(secondaryMeshRenderers, val);

        private static void UpdateColor(List<Renderer> renderers, Color val)
        {
            foreach (var r in renderers) {
                foreach (var mat in r.materials) { mat.color = val; }
            }
        }

        private void OnDestroy()
        {
            foreach (var binding in bindings)
                binding?.Dispose();
        }
    }
}