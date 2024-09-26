using System;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.CollaborationExamples
{
    public class MaterialPickerEntry : MonoBehaviour
    {
        public string MaterialName{ get; private set; }
        
        [SerializeField] private RawImage thumbnail;
        [SerializeField] private GameObject selectedBorder;
        
        private Action<Material> onSelected;

        private Material mat;
        
        public void Setup(Material mat, Action<Material> onSelected)
        {
            MaterialName = mat.name;
            this.mat = mat;
            this.onSelected = onSelected;

            thumbnail.texture = mat.mainTexture;
        }
        
        public void SetSelectionState(bool state)
        {
            selectedBorder.SetActive(state);
        }

        public void Select() => onSelected?.Invoke(mat);
    }
}