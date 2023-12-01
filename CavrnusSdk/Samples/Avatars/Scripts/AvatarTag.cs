using CavrnusSdk.XR.Widgets;
using UnityEngine;

namespace CavrnusSdk.Avatars
{
    public class AvatarTag : MonoBehaviour
    {
        [SerializeField] private WidgetUserProfileImage profileImage;
        [SerializeField] private WidgetUserMic userMic;
        
        private Camera mainCam;

        public void Setup(CavrnusUser user)
        {
            mainCam = Camera.main;
            if (mainCam == null)
                Debug.LogWarning("Missing main cam in scene!");
            
            userMic.Setup(user);
            profileImage.Setup(user);
        }
        
        private void Update()
        {
            if (mainCam != null)
            {
                var dir = transform.position - mainCam.transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
    }
}