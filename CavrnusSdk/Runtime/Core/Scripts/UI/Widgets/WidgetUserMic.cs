using System;
using System.Collections.Generic;
using Collab.Base.Collections;
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

		public void Setup(CavrnusUser cu)
		{
			this.cu = cu;

			micPulse.Setup(cu);

			//Set the mute component to always match the user's data
			var mutedDisposable = cu.IsMuted.Bind(muted => {
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
		}

		public void ToggleMic()
		{
			if (!cu.IsLocalUser) return;

			CavrnusRtcHelpers.SetLocalUserMutedState(CavrnusSpaceJoinEvent.CurrentCavrnusSpace, !cu.IsMuted.Value);
		}

		private void OnDestroy()
		{
			foreach (var disp in disposables)
				disp.Dispose();
		}
	}
}