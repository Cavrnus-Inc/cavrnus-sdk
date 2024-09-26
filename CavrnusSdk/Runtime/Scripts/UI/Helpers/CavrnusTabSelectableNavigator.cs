using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class CavrnusTabSelectableNavigator : MonoBehaviour
    {
        [SerializeField] private List<Selectable> selectables;
        
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
        }
    }
}