using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusAvatarModeSelectorUI : MonoBehaviour
    {
        private CavrnusLivePropertyUpdate<string> liveItemUpdater;
        private CavrnusLivePropertyUpdate<Color> liveColorUpdater;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => 
            {
                spaceConn.AwaitLocalUser(localUser => 
                {
                    liveItemUpdater = spaceConn.BeginTransientStringPropertyUpdate(localUser.ContainerId, "avatarMode", "sphere");
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