using CavrnusSdk.API;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityBase;
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
	            if (profilePicImage == null) {
		            return;
	            }

                profilePicImage.sprite = profilePic;
                if (profilePic != null)
                    profilePicImage.GetComponent<AspectRatioFitter>().aspectRatio =
                        (float)profilePic.texture.width / (float)profilePic.texture.height;
            });
            disposables.Add(picDisp);

            var isStreaming = user.BindUserStreaming(isStreaming => videoStreamImage.gameObject.SetActive(isStreaming));
			disposables.Add(isStreaming);
				
            //Set the profile pic/stream component to always match the user's data
            var videoDisp = user.BindUserVideoFrames(tex => {
                StartCoroutine(AssignVidTexture(tex));
			});
			//Stop matching them up when the menu is destroyed
			disposables.Add(videoDisp);
		}

		private IEnumerator AssignVidTexture(TextureWithUVs tex)
		{
			if (tex.Texture.width > 0 && tex.Texture.height > 0)
				videoStreamImage.GetComponent<AspectRatioFitter>().aspectRatio =
					(float) tex.Texture.width / (float) tex.Texture.height;
			else
				videoStreamImage.GetComponent<AspectRatioFitter>().aspectRatio = 1.5f;
			
			yield return new WaitForSeconds(1f); // Need delay to handle if user is already streaming when loading space

			videoStreamImage.texture = tex.Texture;
			videoStreamImage.uvRect = tex.UVRect;
		}
		
		private void OnDestroy()
		{
			foreach (var disp in disposables) 
				disp.Dispose();
		}
	}
}