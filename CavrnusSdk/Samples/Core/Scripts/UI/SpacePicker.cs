using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CavrnusSdk
{
	public class SpacePicker : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown spacePicker;

		[SerializeField] private TMP_InputField search;

		[Header("This prefab will spawn under the current parent.  It will disappear as soon as the space loads.")]
		[SerializeField]
		private GameObject loadingUiPrefab;

		[Header("These prefabs will spawn as soon as the space loads.")]
		[SerializeField]
		private List<GameObject> spacePrefabs;

		private List<CavrnusSpaceInfo> allSpaces;
		private List<CavrnusSpaceInfo> currentDisplayedSpaces;

		public void Setup(List<CavrnusSpaceInfo> spaces)
		{
			allSpaces = spaces;
			currentDisplayedSpaces = allSpaces;

			var opts = new List<TMP_Dropdown.OptionData>();
			foreach (var space in currentDisplayedSpaces) { opts.Add(new TMP_Dropdown.OptionData(space.Name)); }

			spacePicker.AddOptions(opts);
		}

		public void Search()
		{
			spacePicker.ClearOptions();

			currentDisplayedSpaces = new List<CavrnusSpaceInfo>();
			foreach (var space in allSpaces) {
				if (space.Name.ToLowerInvariant().Contains(search.text.ToLowerInvariant()))
					currentDisplayedSpaces.Add(space);
			}

			var opts = new List<TMP_Dropdown.OptionData>();
			foreach (var space in currentDisplayedSpaces) { opts.Add(new TMP_Dropdown.OptionData(space.Name)); }

			spacePicker.AddOptions(opts);
		}

		public async void JoinSpace()
		{
			if (spacePicker.value < 0 && spacePicker.value >= currentDisplayedSpaces.Count) return;

			var spaceToJoin = currentDisplayedSpaces[spacePicker.value];

			gameObject.SetActive(false);

			var loadingOb = GameObject.Instantiate(loadingUiPrefab, transform.parent);

			var connection = await CavrnusHelpers.JoinSpaceFromId(spaceToJoin.Id);

			GameObject.Destroy(loadingOb);

			foreach (var spacePrefab in spacePrefabs) GameObject.Instantiate(spacePrefab, transform.parent);

			GameObject.Destroy(gameObject);
		}
	}
}