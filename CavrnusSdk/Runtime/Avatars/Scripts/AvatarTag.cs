using System;
using CavrnusSdk.API;
using CavrnusSdk.UI;
using UnityBase;
using UnityEngine;
using CavrnusSdk.Setup;
using TMPro;

namespace CavrnusSdk.Avatars
{
    public class AvatarTag : MonoBehaviour
    {
        [SerializeField] private WidgetUserProfileImage profileImage;
        [SerializeField] private WidgetUserMic userMic;
        
        private Camera mainCam;

        private void Start()
		{
			mainCam = Camera.main;
			if (mainCam == null)
				Debug.LogWarning("Missing main cam in scene!");

            var user = gameObject.GetComponentInAllParents<CavrnusUserFlag>().User;
            //Stop matching them up when the menu is destroyed
            
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