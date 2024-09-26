using System;
using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;
using UnityBase;
using UnityEngine.Events;

namespace CavrnusSdk.UI
{
    public class UiColorPickerMenu : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Color> onColorUpdated;

        [Space]
        [SerializeField] private string containerName;
        [SerializeField] private string propertyName;
        
        [Space]
        [SerializeField] private UiColorPickerEntry colorPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private List<Color> colors;
        
        private readonly List<UiColorPickerEntry> colorItems = new List<UiColorPickerEntry>();
        private CavrnusSpaceConnection spaceConnection;
        private IDisposable binding;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConnection = sc;
                
                foreach (var color in colors) {
                    var go = Instantiate(colorPrefab, container);
                    var colorItem = go.GetComponent<UiColorPickerEntry>();
                    colorItems.Add(colorItem);
                    
                    colorItem.Setup(color, ColorSelected);
                }
                
                binding = spaceConnection.BindColorPropertyValue(containerName, propertyName, serverColor => {
                    onColorUpdated?.Invoke(serverColor);

                    foreach (var item in colorItems) {
                        item.SetSelectionState(ColorsEqual(item.Color, serverColor));
                    }
                });
            });
        }

        private void ColorSelected(Color val)
        {
            spaceConnection?.PostColorPropertyUpdate(containerName, propertyName, val);
        }
        
        private bool ColorsEqual(Color c1, Color c2, float tolerance = 0.1f)
        {
            return c1.r.AlmostEquals(c2.r, tolerance) &&
                   c1.g.AlmostEquals(c2.g, tolerance) &&
                   c1.b.AlmostEquals(c2.b, tolerance);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}