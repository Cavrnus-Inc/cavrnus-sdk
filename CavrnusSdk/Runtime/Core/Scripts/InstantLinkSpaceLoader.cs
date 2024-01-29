using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public class InstantLinkSpaceLoader : MonoBehaviour
	{
		public string InstantLink;
		public string JoinWithUserName;
		
		[Header("These prefabs will spawn as soon as the space loads.")]
		[SerializeField]
		private List<GameObject> spacePrefabs;

		private GameObject spawnedCanvas;
		
		private void Start()
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
			await CavrnusHelpers.JoinLinkAsync(InstantLink, JoinWithUserName);

			foreach (var spacePrefab in spacePrefabs) 
				Instantiate(spacePrefab, null);
		}
	}
}