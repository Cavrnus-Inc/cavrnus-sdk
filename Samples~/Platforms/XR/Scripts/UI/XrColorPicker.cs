using System.Collections.Generic;
using CavrnusSdk.Common;
using UnityEngine;

namespace CavrnusSdk.XR.UI
{
    public class XrColorPicker : MonoBehaviour
    {
        [SerializeField] private List<Color> colors;
        [SerializeField] private GameObject colorPrefab;
        [SerializeField] private Transform container;

        private void Start()
        {
            if (ColorPickerSingleton.Instance == null) {
                Debug.LogWarning($"Missing {nameof(ColorPickerSingleton)} in scene! Color picker will not work.");
                return;
            }
            
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => {
                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    go.GetComponent<XrColorPickerItem>().Setup(color, c => ColorPickerSingleton.Instance.ColorUpdated(c));
                }
            });
        }
    }
}