using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
    public class UiColorPickerMenu : MonoBehaviour
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
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    go.GetComponent<UiColorPickerItem>().Setup(color, c => ColorPickerSingleton.Instance.UpdateColor(c));
                }
            });
        }
    }
}