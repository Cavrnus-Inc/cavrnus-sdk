using System;
using UnityEngine;

namespace CavrnusSdk.Common
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(CavrnusPropertiesContainer))]
    public class ColorPickerSingleton : MonoBehaviour
    {
        public static ColorPickerSingleton Instance{ get; private set; }

        [SerializeField] private Material material;
        
        private MeshRenderer rend;
        private IDisposable disposable;
        private CavrnusSpaceConnection spaceConn;
        private CavrnusPropertiesContainer ctx;

        private void Awake()
        {
            Instance = this;
            
            ctx = GetComponent<CavrnusPropertiesContainer>();
            rend = GetComponent<MeshRenderer>();

            if (material != null)
                rend.sharedMaterial = material;
            else
                Debug.LogWarning($"Missing material on{nameof(ColorPickerSingleton)}"!);
        }

        private void Start()
        {
            if (material != null) {
                CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => {
                    spaceConn = sc;

                    disposable = CavrnusPropertyHelpers.BindToVectorProperty(
                        sc, ctx.UniqueContainerPath, "GenericColorUpdated", c => { rend.sharedMaterial.color = c; });
                });
            }
        }
        
        public void ColorUpdated(Color newValue)
        {
            Vector4 colorVector = newValue;
            if (colorVector != CavrnusPropertyHelpers.GetVectorPropertyValue(spaceConn, ctx.UniqueContainerPath, "GenericColorUpdated")) {
                CavrnusPropertyHelpers.UpdateVectorProperty(spaceConn, ctx.UniqueContainerPath, "GenericColorUpdated", colorVector);
            }
        }
        
        private void OnDestroy() => disposable?.Dispose();
    }
}