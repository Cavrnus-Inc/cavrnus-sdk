using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class TestDynamicProperties : MonoBehaviour
    {
        [SerializeField] private string containerName;
        [SerializeField] private string boolPropertyName;
        [SerializeField] private GameObject target;

        private List<IDisposable> bindings = new List<IDisposable>();
        private CavrnusSpaceConnection spaceConnA;
        private CavrnusSpaceConnection spaceConnB;
        private CavrnusSpaceConnection spaceConnC;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitSpaceBeginLoadingByTag("", msg => {
                print($"Loading TagA!: {msg}");
            });
            
            // remove Any
            CavrnusFunctionLibrary.AwaitSpaceConnectionByTag("",sc => {
                spaceConnA = sc;
                sc.DefineBoolPropertyDefaultValue(containerName, boolPropertyName, true);
                bindings.Add(sc.BindBoolPropertyValue(containerName, boolPropertyName, OnBoolPropUpdated));
                
                sc.AwaitLocalUser(lu => {
                });
            });
        }

        [ContextMenu(nameof(PostBoolToTrue))]
        public void PostBoolToTrue()
        {
            spaceConnA?.PostBoolPropertyUpdate(containerName, boolPropertyName, true);
        }
        
        [ContextMenu(nameof(PostBoolToFalse))]
        public void PostBoolToFalse()
        {
            spaceConnA?.PostBoolPropertyUpdate(containerName, boolPropertyName, false);
        }

        private void OnBoolPropUpdated(bool val)
        {
            PrintPropValue("bool", val.ToString());
            target.SetActive(val);
        }

        private void PrintPropValue(string type, string val)
        {
            print($"Type: {type} : Value: {val}");
        }

        private void OnDestroy()
        {
            bindings.ForEach(b => b?.Dispose());
        }
    }
}