using Collab.Base.Core;
using Collab.RtcCommon;
using System;
using UnityEngine;

namespace CavrnusSdk
{
	public static class CavrnusRtcHelpers
	{
		public static void SetLocalUserMutedState(CavrnusSpaceConnection spaceConn, bool muted)
		{
			spaceConn.RoomSystem.Comm.LocalCommUser.Value.Rtc.Muted.Value = muted;
		}

		public static RtcInputSource CurrAudioInput(CavrnusSpaceConnection spaceConn)
		{
			return spaceConn.RoomSystem.RtcContext.CurrentAudioInputSource.Value;
		}

		public static bool NoAudioInput(CavrnusSpaceConnection spaceConn)
		{
			return spaceConn.RoomSystem.RtcContext.CurrentAudioInputSource.Value == null;
		}

		public static RtcOutputSink CurrAudioOutput(CavrnusSpaceConnection spaceConn)
		{
			return spaceConn.RoomSystem.RtcContext.CurrentAudioOutputSink.Value;
		}

		public static bool NoAudioOutput(CavrnusSpaceConnection spaceConn)
		{
			return spaceConn.RoomSystem.RtcContext.CurrentAudioOutputSink.Value == null;
		}

		public static RtcInputSource CurrVideoInput(CavrnusSpaceConnection spaceConn)
		{
			return spaceConn.RoomSystem.RtcContext.CurrentVideoInputSource.Value;
		}

		public static void UpdateAudioInput(CavrnusSpaceConnection spaceConn, RtcInputSource inputSrc)
		{
			spaceConn.RoomSystem.RtcContext.ChangeAudioInputDevice(inputSrc,
			                                                       (s) => Debug.Log("Changed input device to: " + s),
			                                                       err => DebugOutput.Log(
				                                                       "Failed to change input device: " + err));
		}

		public static void UpdateAudioOutput(CavrnusSpaceConnection spaceConn, RtcOutputSink outputSrc)
		{
			spaceConn.RoomSystem.RtcContext.ChangeAudioOutputDevice(outputSrc,
			                                                        (s) => Debug.Log("Changed output device to: " + s),
			                                                        err => DebugOutput.Log(
				                                                        "Failed to change output device: " + err));
		}

		public static void UpdateVideoInput(CavrnusSpaceConnection spaceConn, RtcInputSource inpSrc)
		{
			spaceConn.RoomSystem.RtcContext.ChangeVideoInputDevice(inpSrc,
			                                                       (s) => Debug.Log(
				                                                       "Changed video input device to: " + s),
			                                                       err => DebugOutput.Log(
				                                                       "Failed to change video input device: " + err));

			spaceConn.RoomSystem.Comm.LocalCommUser.Value.UpdateLocalUserCameraStreamState(inpSrc.Name != "Nothing");
			CavrnusHelpers.DesiredVideoStream.Value = inpSrc;
		}

		public static void FetchAudioInputs(CavrnusSpaceConnection spaceConn, Action<RtcInputSource[]> onFetched)
		{
			spaceConn.RoomSystem.RtcContext.FetchAudioInputOptions(res => {
				CavrnusHelpers.Scheduler.ExecInMainThread(() => onFetched?.Invoke(res));
			});
		}

		public static void FetchAudioOutputs(CavrnusSpaceConnection spaceConn, Action<RtcOutputSink[]> onFetched)
		{
			spaceConn.RoomSystem.RtcContext.FetchAudioOutputOptions(res => {
				CavrnusHelpers.Scheduler.ExecInMainThread(() => onFetched?.Invoke(res));
			});
		}

		public static void FetchVideoInputs(CavrnusSpaceConnection spaceConn, Action<RtcInputSource[]> onFetched)
		{
			spaceConn.RoomSystem.RtcContext.FetchVideoInputOptions(res => {
				CavrnusHelpers.Scheduler.ExecInMainThread(() => onFetched?.Invoke(res));
			});
		}
	}
}