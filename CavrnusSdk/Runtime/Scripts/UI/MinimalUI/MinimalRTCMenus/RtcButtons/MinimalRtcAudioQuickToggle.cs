using System;
using CavrnusSdk.API;
using Collab.Proxy.Prop.JournalInterop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public class MinimalRtcAudioQuickToggle : MonoBehaviour
    {
        public UnityEvent<bool> OnMuteStateChanged;
        
        [SerializeField] private string containerName = UserPropertyDefs.User_Muted;
        [SerializeField] private Button button;
        
        private IDisposable binding;
        private CavrnusUser localUser;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(connection => {
                connection.AwaitLocalUser(lu => {
                    localUser = lu;
                    button.onClick.AddListener(ButtonClicked);
                    binding = connection.BindBoolPropertyValue(lu.ContainerId, containerName, SetButtonState);
                });
            });
        }

        private void ButtonClicked()
        {
            var serverVal = localUser.SpaceConnection.GetBoolPropertyValue(localUser.ContainerId, containerName);
            localUser.SpaceConnection.SetLocalUserMutedState(!serverVal);
            SetButtonState(!serverVal);
        }
        
        public void SetButtonState(bool state)
        {
            OnMuteStateChanged?.Invoke(state);
        }
        
        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}