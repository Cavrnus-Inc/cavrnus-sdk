using Collab.Base.Collections;
using System;
using System.Collections.Generic;
using CavrnusSdk.XR.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk
{
	public class UsersPanelEntry : MonoBehaviour
	{
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private Image profileImage;

		[SerializeField] private WidgetUserMic mic;

		private List<IDisposable> disposables = new List<IDisposable>();

		public void Setup(CavrnusUser user)
		{
			mic.Setup(user);
			
			//Set the name component to always match the user's data
			var nameDisposable = user.UserName.Bind(n => nameText.text = n);
			//Stop matching them up when the menu is destroyed
			disposables.Add(nameDisposable);

			//Set the profile pic/stream component to always match the user's data
			var profileDisposable = user.ProfileAndVideoTexture.Bind(vidTex => {
				profileImage.sprite = vidTex;
				if (vidTex != null)
					profileImage.GetComponent<AspectRatioFitter>().aspectRatio =
						(float) vidTex.texture.width / (float) vidTex.texture.height;
			});
			//Stop matching them up when the menu is destroyed
			disposables.Add(profileDisposable);
		}

		void OnDestroy()
		{
			foreach (var disp in disposables) disp.Dispose();
		}
	}
}