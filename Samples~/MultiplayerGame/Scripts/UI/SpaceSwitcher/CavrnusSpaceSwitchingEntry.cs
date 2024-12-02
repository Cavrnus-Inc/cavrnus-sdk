using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusSpaceSwitchingEntry : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool> onSelectedUnityEvent;
        [SerializeField] private TextMeshProUGUI displayName;
        
        public MultiplayerGame.CavrnusSpaceLevelData.SpaceLevelInfo SpaceData{ get; private set; }
        private Action<CavrnusSpaceSwitchingEntry> onSelected;
        
        public void Setup(MultiplayerGame.CavrnusSpaceLevelData.SpaceLevelInfo data, Action<CavrnusSpaceSwitchingEntry> onSelect)
        {
            SpaceData = data;
            onSelected = onSelect;
            displayName.text = data.SpaceDisplayName;
        }
        
        public void Select() => onSelected?.Invoke(this);

        public void SetSelectedState(bool isSelected)
        {
            onSelectedUnityEvent?.Invoke(isSelected);
        } 
    }
}