using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class CavrnusUIActiveTracker : MonoBehaviour
    {
        public UnityEvent<bool> OnUIActive;

        private bool uiActive;

        private void Update()
        {
            // Check if the Event System has a currently selected game object (UI element)
            if (IsInteractable(EventSystem.current.currentSelectedGameObject))
            {
                if (!uiActive) {
                    OnUIActive?.Invoke(false);
                }
                
                uiActive = true;
            }
            else {
                if (uiActive) {
                    OnUIActive?.Invoke(true);
                }
                
                uiActive = false;
            }

            if (uiActive && !IsPointerOverUI()) {
                if (uiActive) {
                    OnUIActive?.Invoke(true);
                }
                
                uiActive = false;
            }
        }

        private bool IsInteractable(GameObject selectedObject)
        {
            if (selectedObject == null)
                return false;

            var selectable = selectedObject.GetComponent<Selectable>();
            
            return selectable != null && selectable.interactable;
        }
        
        private bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();
    }
}