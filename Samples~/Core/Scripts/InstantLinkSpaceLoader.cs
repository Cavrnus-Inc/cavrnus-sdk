using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public class InstantLinkSpaceLoader : MonoBehaviour
	{
		public string InstantLink;
		public string JoinWithUserName;

		[Header("This canvas is used to display the spawned UI from this loader.")]
		[SerializeField]
		private GameObject canvasUiPrefab;
		
		[Header("This prefab will spawn under the current parent.  It will disappear as soon as the space loads.")]
		[SerializeField]
		private GameObject loadingUiPrefab;

		[Header("These prefabs will spawn as soon as the space loads.")]
		[SerializeField]
		private List<GameObject> spacePrefabs;

		private GameObject spawnedCanvas;
		
		private void Start()
		{
			CavrnusHelpers.Setup();

			if (canvasUiPrefab == null) {
				Debug.LogError($"Add a canvas prefab to the \"Instant Link Space Loader\" prefab to spawn UI.");
				return;
			}

			if (String.IsNullOrEmpty(InstantLink)) {
				Debug.LogError(
					$"Add an Experience Link to the \"Instant Link Space Loader\" prefab to connect to a space.");
				return;
			}

			spawnedCanvas = Instantiate(canvasUiPrefab, null);

			JoinInstantLinkSpace();
		}

		public async void JoinInstantLinkSpace()
		{
			var loadingOb = Instantiate(loadingUiPrefab, spawnedCanvas.transform);

			await CavrnusHelpers.JoinLinkAsync(InstantLink, JoinWithUserName);

			Destroy(loadingOb);

			foreach (var spacePrefab in spacePrefabs) 
				Instantiate(spacePrefab, spawnedCanvas.transform);
		}
	}
}
