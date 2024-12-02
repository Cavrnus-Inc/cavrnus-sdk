using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public abstract class RtcUiDropdownBase : MonoBehaviour
    {
        public event Action<int> OnDropdownValueChanged;
        
        [SerializeField] private Sprite icon;
        [SerializeField] private Image imageContainer;
        
        [SerializeField] protected TMP_Dropdown Dropdown;
        
        protected CavrnusSpaceConnection SpaceConnection;

        private CanvasGroup cg;

        public void Setup()
        {
            cg = gameObject.AddComponent<CanvasGroup>();

            imageContainer.sprite = icon;
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                SpaceConnection = sc;
                
                Dropdown.onValueChanged.AddListener(DropdownValueChanged);
                OnSpaceConnected();
            });
        }

        protected virtual void DropdownValueChanged(int val)
        {
            OnDropdownValueChanged?.Invoke(val);
        }

        public void SetActiveState(bool state)
        {
            gameObject.DoFade(new List<CanvasGroup> {cg}, 0.3f, state);
            gameObject.SetActive(state);
        }

        protected abstract void OnSpaceConnected();
    }
}