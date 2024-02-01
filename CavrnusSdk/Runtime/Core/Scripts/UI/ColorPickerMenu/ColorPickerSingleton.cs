using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.UI
{
    [RequireComponent(typeof(SyncMaterialSharedColor))]
    public class ColorPickerSingleton : MonoBehaviour
    {
        public static ColorPickerSingleton Instance{ get; private set; }

        [SerializeField] private Material material;

        [Space] [SerializeField] private List<Color> colors;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        private int currentColorIndex = 0;

        public void SetNextColor()
        {
            var nextColor = colors[currentColorIndex];
            UpdateColor(nextColor);

            // Move to the next color index and wrap around if needed
            currentColorIndex = (currentColorIndex + 1) % colors.Count;
        }

        public void UpdateColor(Color newValue)
        {
            material.color = newValue;
        }
    }
}