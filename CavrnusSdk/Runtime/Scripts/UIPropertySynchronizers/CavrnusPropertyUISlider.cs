using System;
using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.PropertyUISynchronizers
{
    [RequireComponent(typeof(Slider))]
    public class CavrnusPropertyUISlider : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public Slider Slider{ get; private set; }
        
        private string containerName;
        private string propertyName;

        private IDisposable binding;
        private CavrnusSpaceConnection spaceConn;
        private CavrnusLivePropertyUpdate<float> liveValueUpdate = null;

        private void Awake() => Slider = GetComponent<Slider>();

        public void Setup(string inContainerName, string inPropertyName, Vector2 sliderMinMax, Action<float> onValueUpdated = null)
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                containerName = inContainerName;
                propertyName = inPropertyName;
                
                Slider.minValue = sliderMinMax.x;
                Slider.maxValue = sliderMinMax.y;
                
                spaceConn.DefineFloatPropertyDefaultValue(inContainerName, inPropertyName, 0);
                binding = spaceConn.BindFloatPropertyValue(inContainerName, inPropertyName, val => {
                    Slider.SetValueWithoutNotify(val);
                    onValueUpdated?.Invoke(val);
                });
                
                Slider.onValueChanged.AddListener(OnValueChanged);
            });
        }
        
        private void OnDestroy()
        {
            Slider.onValueChanged.RemoveListener(OnValueChanged);
            binding?.Dispose();
        }

        private void OnValueChanged(float val)
        {
            liveValueUpdate?.UpdateWithNewData(val);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            liveValueUpdate ??= spaceConn.BeginTransientFloatPropertyUpdate(containerName, propertyName, Slider.value);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            liveValueUpdate?.Finish();
            liveValueUpdate = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}