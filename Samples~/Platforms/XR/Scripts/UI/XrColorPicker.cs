using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.XR.UI
{
    [RequireComponent(typeof(CavrnusPropertiesContainer))]
    [RequireComponent(typeof(MeshRenderer))]
    public class XrColorPicker : MonoBehaviour
    {
        [SerializeField] private List<Color> colors;
        [SerializeField] private GameObject colorPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private TextMeshProUGUI textInfo;

        private Renderer material;
        private IDisposable disposable;
        private CavrnusSpaceConnection spaceConn;
        private CavrnusPropertiesContainer ctx;

        private void Start()
        {
            ctx = GetComponent<CavrnusPropertiesContainer>();
            material = GetComponent<MeshRenderer>();

            if (material.sharedMaterial == null) {
                textInfo.text = "Assign material in ColorPicker prefab in order to edit!";
                Debug.LogWarning($"Missing material in material render! Color picker will not work!");
                
                return;
            }

            CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => {
                spaceConn = sc;
                
                disposable = CavrnusPropertyHelpers.BindToVectorProperty(
                    sc, ctx.UniqueContainerPath, "ColorUpdated", c => { material.sharedMaterial.color = c; });

                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    go.GetComponent<XrColorPickerItem>().Setup(color, c => { ColorUpdated(color); });
                }
            });
        }
        
        private void ColorUpdated(Color newValue)
        {
            Vector4 colorVector = newValue;
            if (colorVector !=
                CavrnusPropertyHelpers.GetVectorPropertyValue(spaceConn, ctx.UniqueContainerPath, "ColorUpdated")) {
                CavrnusPropertyHelpers.UpdateVectorProperty(spaceConn, ctx.UniqueContainerPath, "ColorUpdated",
                                                                colorVector);
            }
        }

        private void OnDestroy() => disposable?.Dispose();
    }
}