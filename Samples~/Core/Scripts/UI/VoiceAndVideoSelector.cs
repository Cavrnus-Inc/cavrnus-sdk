using System.Collections.Generic;
using Collab.RtcCommon;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using CavrnusSdk.XR.Widgets;

namespace CavrnusSdk
{
	public class VoiceAndVideoSelector : MonoBehaviour
	{
		[SerializeField] private WidgetUserMic mic;
		[SerializeField] private TMP_Dropdown AudioInputs;
		[SerializeField] private TMP_Dropdown AudioOutputs;
		[SerializeField] private TMP_Dropdown VideoInputs;

		void Start()
		{
			CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);
		}

		RtcInputSource[] inputs;
		RtcOutputSink[] outputs;
		RtcInputSource[] videoInputs;

		CavrnusSpaceConnection spaceConn;

		private const string PLAYERPREFS_AUDIOINPUT = "CavrnusAudioInput";
		private const string PLAYERPREFS_AUDIOOUTPUT = "CavrnusAudioOutput";

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;
			
			var cu = new CavrnusUser(spaceConn.RoomSystem.Comm.LocalCommUser.Value, spaceConn.RoomSystem);
			mic.Setup(cu);

			//If we've already selected audio devices on a previous run, use those
			string savedAudioInput = PlayerPrefs.GetString(PLAYERPREFS_AUDIOINPUT, null);
			string savedAudioOutput = PlayerPrefs.GetString(PLAYERPREFS_AUDIOOUTPUT, null);

			CavrnusRtcHelpers.FetchAudioInputs(spaceConn, opts => {
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
						CavrnusRtcHelpers.UpdateAudioInput(this.spaceConn, selection);
					}
				}
				else if (CavrnusRtcHelpers.NoAudioInput(this.spaceConn) && inputs.Length > 0) {
					AudioInputs.value = 0;
					CavrnusRtcHelpers.UpdateAudioInput(this.spaceConn, inputs[0]);
				}
			});

			CavrnusRtcHelpers.FetchAudioOutputs(spaceConn, opts => {
				//Store the fetched options to look up the selection
				outputs = opts;

				//Clear the existing UI
				AudioOutputs.ClearOptions();

				//Build the dropdown list
				List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
				foreach (var opt in opts) options.Add(new TMP_Dropdown.OptionData(opt.Name));

				//Assign dropdown list to UI
				AudioOutputs.AddOptions(options);

				//If we have a saved selection, pick it
				if (savedAudioOutput != null) {
					var selection = outputs.FirstOrDefault(output => output.Id == savedAudioOutput);

					if (selection != null) {
						AudioOutputs.value = outputs.ToList().IndexOf(selection);
						CavrnusRtcHelpers.UpdateAudioOutput(this.spaceConn, selection);
					}
				}

				if (CavrnusRtcHelpers.NoAudioOutput(this.spaceConn) && outputs.Length > 0) {
					AudioOutputs.value = 0;
					CavrnusRtcHelpers.UpdateAudioOutput(this.spaceConn, outputs[0]);
				}
			});

			CavrnusRtcHelpers.FetchVideoInputs(spaceConn, opts => {
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

			CavrnusRtcHelpers.UpdateAudioInput(spaceConn, inputs[v]);
		}

		public void OutputsChanged(int v)
		{
			//Have we finished fetching the options?
			if (outputs == null) return;

			//Save our selection so we have it on future runs
			PlayerPrefs.SetString(PLAYERPREFS_AUDIOOUTPUT, outputs[v].Id);
			PlayerPrefs.Save();

			CavrnusRtcHelpers.UpdateAudioOutput(spaceConn, outputs[v]);
		}

		public void VideoInputsChanged(int v)
		{
			//Have we finished fetching the options?
			if (videoInputs == null) return;

			CavrnusRtcHelpers.UpdateVideoInput(spaceConn, videoInputs[v]);
		}

		public void ToggleAudioMuted(bool muted) { CavrnusRtcHelpers.SetLocalUserMutedState(spaceConn, muted); }
	}
}