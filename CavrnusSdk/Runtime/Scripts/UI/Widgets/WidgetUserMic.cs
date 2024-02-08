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

		private readonly List<IDisposable> disposables = new List<IDisposable>();

		private CavrnusUser cu;

		private CavrnusSpaceConnection spaceConn;

		public void Setup(CavrnusUser cu)
		{
			this.cu = cu;

			micPulse.Setup(cu);

            //Set the mute component to always match the user's data
            var mutedDisposable = cu.BindUserMuted(muted =>
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

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => this.spaceConn = spaceConn);

			disposables.Add(mutedDisposable);
		}

		public void ToggleMic()
		{
			if (spaceConn == null || !cu.IsLocalUser) return;

			CavrnusFunctionLibrary.SetLocalUserMutedState(spaceConn, !cu.GetUserMuted());
		}

		private void OnDestroy()
		{
			foreach (var disp in disposables)
				disp.Dispose();
		}
	}
}