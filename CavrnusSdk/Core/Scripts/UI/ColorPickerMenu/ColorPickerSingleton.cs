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

        public void SetNewDefinedColor()
        {
            var color = colors.IndexOf(material.color);

            // Is last in list? 
            if (color + 1 == colors.Count)
                color = 0;
            else
                color += 1;

            UpdateColor(colors[color]);
        }

        public void UpdateColor(Color newValue) { material.color = newValue; }
    }
}