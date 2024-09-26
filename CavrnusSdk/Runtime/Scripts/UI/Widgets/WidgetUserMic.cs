using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.UI
{
	public class WidgetUserMic : MonoBehaviour
	{
		[SerializeField] private GameObject speakingGameObject;
		[SerializeField] private GameObject mutedGameObject;

		[SerializeField] private WidgetMicPulse micPulse;

		private CavrnusUser user;
		private CavrnusSpaceConnection spaceConn;
		private readonly List<IDisposable> disposables = new List<IDisposable>();

		private void Start()
		{
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(connection => {
				connection.AwaitLocalUser(lu => {
					user = lu;
					spaceConn = user.SpaceConnection;
					micPulse.Setup(lu);

					//Set the mute component to always match the user's data
					var mutedDisposable = lu.BindUserMuted(muted =>
					{
						if (muted)
						{
							speakingGameObject.SetActive(false);
							mutedGameObject.SetActive(true);
						}
						else
						{
							speakingGameObject.SetActive(true);
							mutedGameObject.SetActive(false);
						}
					});

					disposables.Add(mutedDisposable);
				});
			});
		}

		public void ToggleMic()
		{
			if (!user.IsLocalUser) return;

			spaceConn?.SetLocalUserMutedState(!user.GetUserMuted());
		}

		private void OnDestroy()
		{
			foreach (var disp in disposables)
				disp.Dispose();
		}
	}
}