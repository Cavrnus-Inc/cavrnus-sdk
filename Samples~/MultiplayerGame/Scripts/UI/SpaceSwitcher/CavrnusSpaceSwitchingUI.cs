using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusSpaceSwitchingUI : MonoBehaviour
    {
        [SerializeField] private MultiplayerGame.CavrnusSpaceLevelData cavrnusSpaceLevelData;
                
        [Space]
        [SerializeField] private CavrnusSpaceSwitchingEntry entryPrefab;
        [SerializeField] private Transform entriesContainer;

        private CavrnusSpaceSwitchingEntry currentSelectedEntry;
        private CavrnusSpaceConnection spaceConnection;

        public void Start()
        {
            CavrnusFunctionLibrary.AwaitAuthentication(auth => {
                SetupUI();    
            });
        }

        private void SetupUI()
        {
            cavrnusSpaceLevelData.Levels.ForEach(data => {
                var entry = Instantiate(entryPrefab, entriesContainer, false);
                entry.Setup(data, SpaceSelected);
            });
        }

        private void SpaceSelected(CavrnusSpaceSwitchingEntry entry)
        {
            if (currentSelectedEntry != null)
                currentSelectedEntry.SetSelectedState(false);
            
            entry.SetSelectedState(true);
            currentSelectedEntry = entry;
            
            cavrnusSpaceLevelData.LoadLevel(spaceConnection, entry.SpaceData, OnSpaceLevelLoaded);
        }

        private void OnSpaceLevelLoaded()
        {
            
        }
    }
}