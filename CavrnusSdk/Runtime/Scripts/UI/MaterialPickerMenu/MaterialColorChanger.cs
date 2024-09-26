using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
    public class MaterialColorChanger : MonoBehaviour
    {
        [SerializeField] private List<Color> colors;
        [SerializeField] private GameObject colorPrefab;
        [SerializeField] private Transform container;
        
        [Header("Optionally set via renderer")]
        [SerializeField] private Renderer rend;
        [SerializeField] private bool useSharedMaterial;

        [Header("Optionally set via material")]
        [SerializeField] private Material material;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    go.GetComponent<UiColorPickerEntry>().Setup(color, c => {

                        if (rend != null) {
                            if (useSharedMaterial) {
                                rend.sharedMaterial.color = c;
                            }
                            else {
                                rend.material.color = c;
                            }
                        }

                        if (material != null)
                            material.color = c;
                    });
                }
            });
        }
    }
}