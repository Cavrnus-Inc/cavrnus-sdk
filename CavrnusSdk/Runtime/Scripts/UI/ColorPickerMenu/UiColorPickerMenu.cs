using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;
using UnityEngine.Events;

namespace CavrnusSdk.UI
{
    public class UiColorPickerMenu : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Color> onColorUpdated;
        
        [SerializeField] private List<Color> colors;
        [SerializeField] private GameObject colorPrefab;
        [SerializeField] private Transform container;

        [Space]
        [SerializeField] private Material targetMaterial;
        [SerializeField] private string materialColorPropertyName = "_BaseColor";

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    go.GetComponent<UiColorPickerItem>().Setup(color, c => {
                        targetMaterial.SetColor(materialColorPropertyName, c);
                        onColorUpdated?.Invoke(c);
                    });
                }
            });
        }
    }
}