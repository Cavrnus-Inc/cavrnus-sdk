using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class DirectionalLightMenu : MonoBehaviour
    {
        [SerializeField] private string containerName = "DirectionalLight";

        [Header("Color Sync Properties")]
        [SerializeField] private string rotationPropertyName = "SunlightRotation";
        [SerializeField] private string shadowPropertyName = "ShadowStrength";
        
        [Header("Components")]
        [SerializeField] private GameObject lightContainer;
        [SerializeField] private Light targetLight;
        
        [Header("UI")]
        [SerializeField] private UISliderWrapper rotationSlider;
        [SerializeField] private UISliderWrapper shadowSlider;

        private List<IDisposable> disposables = new List<IDisposable>();
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                rotationSlider.Setup(spaceConn, containerName, rotationPropertyName, new Vector2(0f, 360f));
                shadowSlider.Setup(spaceConn, containerName, shadowPropertyName, new Vector2(0f, 1f));

                // Sunlight Rotation
                spaceConn.DefineFloatPropertyDefaultValue(containerName, rotationPropertyName, RenderSettings.skybox.GetFloat(Rotation));
                disposables.Add(spaceConn.BindFloatPropertyValue(containerName, rotationPropertyName, rotation => {
                    RenderSettings.skybox.SetFloat(Rotation, rotation);
                    lightContainer.transform.localRotation = Quaternion.Euler(new Vector3(0, -rotation, 0));
                    rotationSlider.Slider.SetValueWithoutNotify(rotation);
                }));
                
                // Shadow Strength
                spaceConn.DefineFloatPropertyDefaultValue(containerName, shadowPropertyName, targetLight.shadowStrength);
                disposables.Add(spaceConn.BindFloatPropertyValue(containerName, shadowPropertyName, strength => {
                    targetLight.shadowStrength = strength;
                    shadowSlider.Slider.SetValueWithoutNotify(strength);
                }));
            });
        }

        private void OnDestroy()
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }
}