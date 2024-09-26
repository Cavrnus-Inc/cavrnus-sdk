using System;
using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.PropertyUISynchronizers
{
    [RequireComponent(typeof(Toggle))]
    public class CavrnusPropertyUIToggle : MonoBehaviour
    {
        private string containerName;
        private string propertyName;
        private bool isTransient = false;

        private Toggle toggle;

        private IDisposable binding;
        private CavrnusSpaceConnection spaceConn;
        private CavrnusLivePropertyUpdate<bool> updater;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
        }
        
        public void Setup(CavrnusSpaceConnection inSpaceConn, string inContainerName, string inPropertyName, bool inIsTransient = false,  Action<bool> onValueUpdated = null)
        {
            spaceConn = inSpaceConn;
            containerName = inContainerName;
            propertyName = inPropertyName;
            isTransient = inIsTransient;
            
            spaceConn.DefineBoolPropertyDefaultValue(containerName, propertyName,false);
            binding = inSpaceConn.BindBoolPropertyValue(inContainerName, inPropertyName, val => {
                toggle.isOn = val;
                onValueUpdated?.Invoke(val);
            });
            
            toggle.onValueChanged.AddListener(OnToggleClicked);
        }

        private void OnToggleClicked(bool val)
        {
            if (isTransient) {
                if (updater == null)
                    updater = spaceConn.BeginTransientBoolPropertyUpdate(containerName, propertyName, val);
                else
                    updater?.UpdateWithNewData(val);
            }
            else
                spaceConn?.PostBoolPropertyUpdate(containerName, propertyName, val);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggleClicked);

            updater = null;
            binding?.Dispose();
        }
    }
}