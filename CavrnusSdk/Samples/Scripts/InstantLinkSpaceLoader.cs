using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public class InstantLinkSpaceLoader : MonoBehaviour
	{
		public string InstantLink;
		public string JoinWithUserName;

		[Header("This prefab will spawn under the current parent.  It will disappear as soon as the space loads.")]
		[SerializeField]
		private GameObject loadingUiPrefab;

		[Header("These prefabs will spawn as soon as the space loads.")]
		[SerializeField]
		private List<GameObject> spacePrefabs;

		// Start is called before the first frame update
		void Start()
		{
			CavrnusHelpers.Setup();

			if (String.IsNullOrEmpty(InstantLink)) {
				Debug.LogError(
					$"Add an Experience Link to the \"Instant Link Space Loader\" prefab to connect to a space.");
				return;
			}

			JoinInstantLinkSpace();
		}

		public async void JoinInstantLinkSpace()
		{
			var loadingOb = GameObject.Instantiate(loadingUiPrefab, transform.parent);

			await CavrnusHelpers.JoinLinkAsync(InstantLink, JoinWithUserName);

			GameObject.Destroy(loadingOb);

			foreach (var spacePrefab in spacePrefabs) GameObject.Instantiate(spacePrefab, transform.parent);
		}
	}
}