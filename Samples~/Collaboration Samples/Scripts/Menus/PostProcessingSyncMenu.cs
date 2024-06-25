using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace CavrnusSdk.CollaborationExamples
{
    public class PostProcessingSyncMenu : MonoBehaviour
    {
        [SerializeField] private Volume volume;

        [Header("Container")]
        [SerializeField] private string containerName = "PostProcessing";

        [Header("Saturation")]
        [SerializeField] private string saturationEnabledPropertyName = "SaturationEnabled";
        [SerializeField] private string saturationValuePropertyName = "Saturation";
        [SerializeField] private Toggle saturationToggle;
        [SerializeField] private UISliderWrapper saturationSlider;
        
        [Header("Saturation")]
        [SerializeField] private string bloomEnabledPropertyName = "BloomEnabled";
        [SerializeField] private string bloomValuePropertyName = "Bloom";
        [SerializeField] private Toggle bloomShiftToggle;
        [SerializeField] private UISliderWrapper bloomShiftSlider;

        private CavrnusSpaceConnection spaceConn;
        private List<IDisposable> disposables = new List<IDisposable>();
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                this.spaceConn = spaceConn;

                if (volume == null) {
                    volume = FindObjectOfType<Volume>();

                    if (volume == null) {
                        Debug.LogWarning($"Missing PostProcessing Volume in Scene!");
                        return;
                    }
                }
           
                if (volume.profile.TryGet(out ColorAdjustments ca))
                    SetupSaturationProperties(spaceConn, ca);
                
                if (volume.profile.TryGet(out Bloom b))
                    SetupBloomShiftProperties(spaceConn, b);
            });
        }
        
        #region Saturation Property
        
        private void SetupSaturationProperties(CavrnusSpaceConnection spaceConn, ColorAdjustments ca)
        {
            saturationSlider.Setup(spaceConn, containerName, saturationValuePropertyName, new Vector2(-100, 100));
            saturationToggle.onValueChanged.AddListener(SaturationToggleUIUpdated);

            spaceConn.DefineBoolPropertyDefaultValue(containerName, saturationEnabledPropertyName, ca.saturation.overrideState);
            disposables.Add(spaceConn.BindBoolPropertyValue(containerName, saturationEnabledPropertyName, val => {
                ca.saturation.overrideState = val;
                saturationToggle.SetIsOnWithoutNotify(val);
            }));

            spaceConn.DefineFloatPropertyDefaultValue(containerName, saturationValuePropertyName, ca.saturation.value);
            disposables.Add(spaceConn.BindFloatPropertyValue(containerName, saturationValuePropertyName, val => {
                ca.saturation.value = val;
                saturationSlider.Slider.SetValueWithoutNotify(val);
            }));
        }

        private void SaturationToggleUIUpdated(bool val)
        {
            spaceConn?.PostBoolPropertyUpdate(containerName, saturationEnabledPropertyName, val);
        }

        #endregion

        #region Bloom Property

        private void SetupBloomShiftProperties(CavrnusSpaceConnection spaceConn, Bloom bloom)
        {
            bloomShiftSlider.Setup(spaceConn, containerName, bloomValuePropertyName, new Vector2(-100, 100));
            bloomShiftToggle.onValueChanged.AddListener(BloomShiftToggleUpdated);

            spaceConn.DefineBoolPropertyDefaultValue(containerName, bloomEnabledPropertyName, bloom.active);
            disposables.Add(spaceConn.BindBoolPropertyValue(containerName, bloomEnabledPropertyName, val => {
                bloom.active = val;
                bloomShiftToggle.SetIsOnWithoutNotify(val);
            }));

            spaceConn.DefineFloatPropertyDefaultValue(containerName, bloomValuePropertyName, bloom.intensity.value);
            disposables.Add(spaceConn.BindFloatPropertyValue(containerName, bloomValuePropertyName, val => {
                bloom.intensity.value = val;
                bloomShiftSlider.Slider.SetValueWithoutNotify(val);
            }));
        }
   
        private void BloomShiftToggleUpdated(bool val)
        {
            spaceConn?.PostBoolPropertyUpdate(containerName, bloomEnabledPropertyName, val);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
            
            bloomShiftToggle.onValueChanged.RemoveListener(BloomShiftToggleUpdated);
            saturationToggle.onValueChanged.RemoveListener(SaturationToggleUIUpdated);
        }
    }
}