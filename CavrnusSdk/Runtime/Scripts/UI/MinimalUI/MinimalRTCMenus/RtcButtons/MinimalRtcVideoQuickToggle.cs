using System;
using CavrnusSdk.API;
using Collab.Proxy.Prop.JournalInterop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class MinimalRtcVideoQuickToggle : MonoBehaviour
    {
        public UnityEvent<bool> OnStreamingStateChanged;
        
        [SerializeField] private string containerName = UserPropertyDefs.User_Streaming;
        [SerializeField] private Button button;
        
        [SerializeField] private RtcUiDropdownBase dropdown;

        private IDisposable binding;
        private CavrnusUser localUser;

        private int currentDropdownSelection = 0;

        private void Start()
        {
            dropdown.OnDropdownValueChanged += DropdownValueChanged;

            SetButtonState(false);
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConn.AwaitLocalUser(lu => {
                    localUser = lu;
                    button.onClick.AddListener(ButtonClicked);
                    
                    spaceConn.DefineBoolPropertyDefaultValue(lu.ContainerId, containerName, false);
                    binding = spaceConn.BindBoolPropertyValue(lu.ContainerId, containerName, StreamingModeChanged);
                });
            });
        }

        private void ButtonClicked()
        {
            var serverVal = localUser.SpaceConnection.GetBoolPropertyValue(localUser.ContainerId, containerName);
            localUser.SpaceConnection.SetLocalUserStreamingState(!serverVal);
            SetButtonState(!serverVal);
        }

        private void DropdownValueChanged(int selection)
        {
            currentDropdownSelection = selection;
        }

        public void StreamingModeChanged(bool state)
        {
            if (!state && currentDropdownSelection == 0) {
                return;
            }
            
            SetButtonState(state);
        }

        public void SetButtonState(bool state)
        {
            OnStreamingStateChanged?.Invoke(state);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
            dropdown.OnDropdownValueChanged -= DropdownValueChanged;
        }
    }
}