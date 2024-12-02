using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusAvatarModeSelectorUI : MonoBehaviour
    {
        private CavrnusLivePropertyUpdate<string> liveItemUpdater;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => 
            {
                spaceConn.AwaitLocalUser(localUser =>
                {
	                localUser.BindUserContainerId(containerId =>
	                {
		                liveItemUpdater = spaceConn.BeginTransientStringPropertyUpdate(containerId, "avatarMode", "sphere");
	                });
                });
            });
        }

        public void SelectSphereMode()
        {
            liveItemUpdater?.UpdateWithNewData("sphere");
        }

        public void SelectCubeMode()
        {
            liveItemUpdater?.UpdateWithNewData("cube");
        }
    }
}