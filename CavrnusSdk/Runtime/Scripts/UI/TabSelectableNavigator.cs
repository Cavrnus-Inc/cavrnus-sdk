using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class TabSelectableNavigator : MonoBehaviour
    {
        [SerializeField] private List<Selectable> selectables;
        [SerializeField] private Button ctaButton;
        
        private EventSystem eventSystem;

        private void Start() {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                for (var i = 0; i < selectables.Count; i++) {
                    if(selectables[i].gameObject == eventSystem.currentSelectedGameObject) {
                        selectables[(i+1) % selectables.Count].Select();
                        break;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
                if (ctaButton != null)
                    ExecuteEvents.Execute(ctaButton.gameObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
            }
        }
    }
}