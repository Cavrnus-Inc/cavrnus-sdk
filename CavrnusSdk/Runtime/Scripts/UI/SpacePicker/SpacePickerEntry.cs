using System;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
    public class SpacePickerEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI spaceNameText;
        
        private CavrnusSpaceInfo content;
        private Action<CavrnusSpaceInfo> selected;
        
        public void Setup(CavrnusSpaceInfo content, Action<CavrnusSpaceInfo> selected)
        {
            this.content = content;
            this.selected = selected;
            
            spaceNameText.text = content.Name;
        }

        public void Select()
        {
            selected?.Invoke(content);
        }
    }
}