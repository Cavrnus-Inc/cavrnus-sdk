using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
	public class SpacePicker : MonoBehaviour
	{
		private class SpacePickerOption : IListElement
		{
			private readonly CavrnusSpaceInfo content;
			private readonly Action<CavrnusSpaceInfo> selected;
			
			public SpacePickerOption(CavrnusSpaceInfo content, Action<CavrnusSpaceInfo> selected)
			{
				this.content = content;
				this.selected = selected;
			}
			
			public void EntryBuilt(GameObject element)
			{
				element.GetComponent<SpacePickerEntry>().Setup(content, selected);
			}
		}
		
		[SerializeField] private TMP_InputField search;
		[SerializeField] private GameObject spacePickerPrefab;
		[SerializeField] private Pagination pagination;
		
		private List<CavrnusSpaceInfo> allSpaces;
		private List<CavrnusSpaceInfo> currentDisplayedSpaces;

		private void Start()
		{
			search.interactable = false;

			CavrnusFunctionLibrary.FetchJoinableSpaces(spaces =>
			{
				allSpaces = spaces;
				currentDisplayedSpaces = allSpaces;

				search.interactable = true;
				search.onValueChanged.AddListener(Search);
			});		
		}

		public void Search(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) {
				pagination.ResetPagination();
				currentDisplayedSpaces.Clear();
				
				return;
			}
			
			currentDisplayedSpaces = new List<CavrnusSpaceInfo>();
			foreach (var space in allSpaces) {
				if (space.Name.ToLowerInvariant().Contains(value.ToLowerInvariant()))
					currentDisplayedSpaces.Add(space);
			}

			var options = new List<IListElement>();
			currentDisplayedSpaces.ForEach(s => options.Add(new SpacePickerOption(s,JoinSelectedSpace)));
			
			pagination.NewPagination(spacePickerPrefab, options);
		}

		private void JoinSelectedSpace(CavrnusSpaceInfo csi)
		{
			CavrnusFunctionLibrary.JoinSpace(csi.Id, (spaceConn) => {
				/*The Post-Load cleanup is done by the Cavrnus Spatial Connector.
				 If you did you own version though, you would need to implement this*/
			}, err => Debug.LogError(err));
		}

		private void OnDestroy()
		{
			search.onValueChanged.RemoveListener(Search);
		}
	}
}