using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusStringSelectionItem : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool> onUnitySelectedState;

        public string StringValue{ get; private set; }

        [SerializeField] private TextMeshProUGUI tmPro;
        
        private Action<string> onSelected;

        public void Setup(string stringVal, Action<string> onSelected)
        {
            StringValue = stringVal;
            this.onSelected = onSelected;
            tmPro.text = stringVal;
            
            SetSelectionState(false);
        }
        
        public void SetSelectionState(bool state)
        {
            onUnitySelectedState?.Invoke(state);
        }

        public void Select()
        {
            onSelected?.Invoke(StringValue);
        }
    }
}