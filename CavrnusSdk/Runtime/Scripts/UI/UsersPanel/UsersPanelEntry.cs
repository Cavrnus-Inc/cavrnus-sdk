using CavrnusSdk.API;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
	public class UsersPanelEntry : MonoBehaviour
	{
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private RawImage videoStreamImage;
		[SerializeField] private Image profilePicImage;

		[SerializeField] private WidgetUserMic mic;

		private List<IDisposable> disposables = new List<IDisposable>();

		public void Setup(CavrnusUser user)
		{
			mic.Setup(user);
			
			//Set the name component to always match the user's data
			var nameDisposable = user.BindUserName(n => nameText.text = n);
			//Stop matching them up when the menu is destroyed
			disposables.Add(nameDisposable);

            var picDisp = user.BindProfilePic(this, profilePic =>
            {
                profilePicImage.sprite = profilePic;
                if (profilePic != null)
                    profilePicImage.GetComponent<AspectRatioFitter>().aspectRatio =
                        (float)profilePic.texture.width / (float)profilePic.texture.height;
            });
            disposables.Add(picDisp);

            //Set the profile pic/stream component to always match the user's data
            var videoDisp = user.BindUserVideoFrames(tex => {

                videoStreamImage.texture = tex.Texture;
                videoStreamImage.uvRect = tex.UVRect;

                if (tex.Texture.width > 0 && tex.Texture.height > 0)
                    videoStreamImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.Texture.width / (float)tex.Texture.height;
                else
                    videoStreamImage.GetComponent<AspectRatioFitter>().aspectRatio = 1.5f;
			});
			//Stop matching them up when the menu is destroyed
			disposables.Add(videoDisp);
		}

		void OnDestroy()
		{
			foreach (var disp in disposables) disp.Dispose();
		}
	}
}