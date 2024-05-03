using System;
using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.CollaborationExamples
{
    public class UISliderWrapper : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public event Action<float> OnValueUpdated;         
        public event Action<float> OnBeginDragging;         
        public event Action<float> OnEndDragging;

        public Slider Slider => slider;
        [SerializeField] private Slider slider;
        
        private CavrnusLivePropertyUpdate<float> liveValueUpdate = null;

        private CavrnusSpaceConnection spaceConn;
        private string containerName;
        private string propertyName;

        public void Setup(CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector2 sliderMinMax)
        {
            this.spaceConn = spaceConn;
            this.containerName = containerName;
            this.propertyName = propertyName;

            Slider.minValue = sliderMinMax.x;
            Slider.maxValue = sliderMinMax.y;
        }
        
        private void Awake()
        {
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float val)
        {
            OnValueUpdated?.Invoke(val);
            liveValueUpdate?.UpdateWithNewData(val);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragging?.Invoke(Slider.value);
            liveValueUpdate ??= spaceConn.BeginTransientFloatPropertyUpdate(containerName, propertyName, Slider.value);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragging?.Invoke(Slider.value);
            liveValueUpdate?.Finish();
            liveValueUpdate = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}