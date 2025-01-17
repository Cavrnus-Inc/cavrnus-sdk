using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
	public class VoiceAndVideoSelector : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown AudioInputs;
		[SerializeField] private TMP_Dropdown VideoInputs;

		private void Start()
		{
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection);
		}

		List<CavrnusInputDevice> inputs;
		List<CavrnusVideoInputDevice> videoInputs;

		CavrnusSpaceConnection spaceConn;

		private const string PLAYERPREFS_AUDIOINPUT = "CavrnusAudioInput";

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;

			//If we've already selected audio devices on a previous run, use those
			string savedAudioInput = PlayerPrefs.GetString(PLAYERPREFS_AUDIOINPUT, null);

			spaceConn.FetchAudioInputs(opts => {
				//Store the fetched options to look up the selection
				inputs = opts;

				//Clear the existing UI
				AudioInputs.ClearOptions();

				//Build the dropdown list
				List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
				foreach (var opt in opts) options.Add(new TMP_Dropdown.OptionData(opt.Name));

				//Assign dropdown list to UI
				AudioInputs.AddOptions(options);

				//If we have a saved selection, pick it
				if (savedAudioInput != null) {
					var selection = inputs.FirstOrDefault(inp => inp.Id == savedAudioInput);

					if (selection != null) {
						AudioInputs.value = inputs.ToList().IndexOf(selection);
						spaceConn.UpdateAudioInput(selection);
					}
				}
				else if (inputs.Count > 0) {
					AudioInputs.value = 0;
					spaceConn.UpdateAudioInput(inputs[0]);
				}
			});

			spaceConn.FetchVideoInputs(opts => {
				//Store the fetched options to look up the selection
				videoInputs = opts;

				//Clear the existing UI
				VideoInputs.ClearOptions();

				//Build the dropdown list
				List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
				foreach (var opt in opts) options.Add(new TMP_Dropdown.OptionData(opt.Name));

				//Assign dropdown list to UI
				VideoInputs.AddOptions(options);
				VideoInputs.value = 0;
			});
		}

		public void InputsChanged(int v)
		{
			//Have we finished fetching the options?
			if (inputs == null) return;

			//Save our selection so we have it on future runs
			PlayerPrefs.SetString(PLAYERPREFS_AUDIOINPUT, inputs[v].Id);
			PlayerPrefs.Save();

			spaceConn.UpdateAudioInput(inputs[v]);
		}

		public void VideoInputsChanged(int v)
		{
			//Have we finished fetching the options?
			if (videoInputs == null) {
				spaceConn.SetLocalUserStreamingState(false);

				return;
			}

			spaceConn.UpdateVideoInput(videoInputs[v]);
			
			spaceConn.SetLocalUserStreamingState(true);
		}

		public void ToggleAudioMuted(bool muted) { CavrnusFunctionLibrary.SetLocalUserMutedState(spaceConn, muted); }
	}
}