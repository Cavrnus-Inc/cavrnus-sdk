using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
	public class SpacePicker : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown spacePicker;

		[SerializeField] private TMP_InputField search;

		private List<CavrnusSpaceInfo> allSpaces;
		private List<CavrnusSpaceInfo> currentDisplayedSpaces;

		void Start()
		{
			Setup();
		}

		private void Setup()
		{
			CavrnusFunctionLibrary.FetchJoinableSpaces(spaces =>
			{
				allSpaces = spaces;
				currentDisplayedSpaces = allSpaces;

				var opts = new List<TMP_Dropdown.OptionData>();
				foreach (var space in currentDisplayedSpaces) { opts.Add(new TMP_Dropdown.OptionData(space.Name)); }

				spacePicker.AddOptions(opts);
			});			
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

		public void JoinSpace()
		{
			if (spacePicker.value < 0 && spacePicker.value >= currentDisplayedSpaces.Count) return;

			var spaceToJoin = currentDisplayedSpaces[spacePicker.value];

			CavrnusFunctionLibrary.JoinSpace(spaceToJoin.Id, (spaceConn) =>{/*The Post-Load cleanup is done by the Cavrnus Spatial Connector.  If you did you own version though, you would need to implement this*/}, err => Debug.LogError(err));
		}
	}
}