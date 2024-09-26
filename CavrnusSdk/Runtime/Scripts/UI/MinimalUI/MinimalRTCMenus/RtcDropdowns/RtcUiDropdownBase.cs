using System;
using System.Collections.Generic;
using CavrnusCore;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.UI.MinimalUI
{
    public abstract class RtcUiDropdownBase : MonoBehaviour
    {
        public event Action<int> OnDropdownValueChanged;
        public event Action OnUserClickedOffThisDropdown;
        
        [SerializeField] private Sprite icon;
        [SerializeField] private Image imageContainer;
        
        [SerializeField] protected TMP_Dropdown Dropdown;
        
        protected CavrnusSpaceConnection SpaceConnection;

        private CanvasGroup cg;

        private void Awake()
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
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
            CavrnusStatics.Scheduler.StartCoroutine(this.DoFade(new List<CanvasGroup> {cg}, 0.3f, state));
            gameObject.SetActive(state);
        }

        protected abstract void OnSpaceConnected();
        
        private void Update()
        {
            // DetectClickOutside();
        }

        // Method to detect clicks outside the dropdown
        private void DetectClickOutside()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse click
            {
                // Check if pointer is over a UI element
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    // Pointer is over a UI element, check if it's this dropdown or its children
                    if (!IsPointerOverDropdown())
                    {
                        HandleClickOutside();
                    }
                }
                else
                {
                    // If the click is not over any UI element, it's outside the dropdown
                    HandleClickOutside();
                }
            }
        }

        // Check if the pointer is over the dropdown or its children
        private bool IsPointerOverDropdown()
        {
            // Check if the pointer is over this gameobject or any of its children
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
        }

        // Handle the event when a click is detected outside the dropdown
        private void HandleClickOutside()
        {
            OnUserClickedOffThisDropdown?.Invoke();
        }
    }
}