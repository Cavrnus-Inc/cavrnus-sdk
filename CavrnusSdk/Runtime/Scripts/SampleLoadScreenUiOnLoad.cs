using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk
{
    public class SampleLoadScreenUiOnLoad : MonoBehaviour
    {
        [Header("This canvas is used to display the spawned UI from this loader.")]
        [SerializeField] private GameObject canvasUiPrefab;
        
        [Header("This prefab will spawn under the current parent.  It will disappear as soon as the space loads.")]
        [SerializeField]
        private GameObject loadingUiPrefab;
        
        [Space]
        [SerializeField] private List<GameObject> uiToLoad;
        
        private CavrnusSpaceConnection spaceConn;

        private void Start()
        {
            var spawnedCanvas = Instantiate(canvasUiPrefab, null);
            var spawnedLoadingPrefab = Instantiate(loadingUiPrefab, spawnedCanvas.transform);

            CavrnusFunctionLibrary.AwaitAnySpaceConnection(csc => {
                Destroy(spawnedLoadingPrefab);
                
                foreach (var ui in uiToLoad)
                    Instantiate(ui, spawnedCanvas.transform);
            });
        }
    }
}