using Assets.Scripts;
using Collab.Base.Core;
using Collab.Proxy;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Collab.Proxy.Comm;
using Collab.Proxy.Comm.NotifyApi;
using Collab.RtcCommon;
using Collab.Base.Collections;
using CavrnusSdk;
using CavrnusSdk.Setup;
using static CavrnusSdk.Setup.CavrnusSpatialConnector;
using System.IO;
using System;

namespace CavrnusCore
{
	public static class CavrnusStatics
	{
		public static UnityScheduler Scheduler
		{
			get {
				if (scheduler == null) {
					scheduler = new GameObject("Scheduler").AddComponent<UnityScheduler>();
					scheduler.Setup();
				}

				return scheduler;
			}
		}

		private static UnityScheduler scheduler = null;
		internal static NotifyCommunication Notify { get; private set; }
		internal static LivePolicyEvaluator LivePolicyEvaluator { get; private set; }

		internal static MultiTranslatingSetting<bool, bool, bool> NotifyPoliciesAndRolesFullyLoaded{ get; set; }

		internal static INetworkRequestImplementation NetworkRequestImpl;
		internal static IRtcContext RtcContext;

		internal static ISetting<RtcInputSource> DesiredVideoStream = new Setting<RtcInputSource>(null);

		internal static void Setup(CavrnusSettings settings)
		{
			UnityBase.HelperFunctions.MainThread = Thread.CurrentThread;
			DebugOutput.MessageEvent += DoRecvMessage;
			CollabPaths.FlushTemporaryFilePath();

			HandlePlatformsSetup();

			NetworkRequestImpl = new FrameworkNetworkRequestImplementation();

			Notify = new NotifyCommunication(() => new NotifyWebsocket(Scheduler.BaseScheduler), Scheduler.BaseScheduler);
			LivePolicyEvaluator = new LivePolicyEvaluator(Notify.PoliciesSystem.AllPolicies, Notify.PoliciesSystem.IsActive);
			
			IRtcSystem rtcSystem;
			
			if (settings.DisableVoiceAndVideo) 
			{ 
				rtcSystem = new RtcSystemUnavailable(); 
			}
			else
			{
				rtcSystem = new RtcSystemUnity(Scheduler, settings.DisableAcousticEchoCancellation);
			}

			RtcContext = new RtcContext(rtcSystem, Scheduler.BaseScheduler);

			var input = RtcInputSource.FromJson("");
			var output = RtcOutputSink.FromJson("");
			var vidInput = RtcInputSource.FromJson("");

			RtcContext.Initialize(input, output, vidInput, RtcModeEnum.AudioVideo, RtcModeEnum.AudioVideo);

			RtcContext.CurrentAudioInputSource.Bind(src =>
				                                        DebugOutput.Info(
					                                        "RTC Input Source is now '" + src?.ToJson() + "'."));
			RtcContext.CurrentAudioOutputSink.Bind(src =>
				                                       DebugOutput.Info(
					                                       "RTC Output Sink is now '" + src?.ToJson() + "'."));
			RtcContext.CurrentVideoInputSource.Bind(src =>
				                                        DebugOutput.Info(
					                                        "RTC Video Source is now '" + src?.ToJson() + "'."));

			Scheduler.ExecOnApplicationQuit(() => RtcContext.Shutdown());
		}

		private static void DoRecvMessage(string category, string message)
		{
			if (category == "log") return; // Ignore log messages.

			string callstack = "";
			callstack = "\r\n@ " + new StackTrace(1, true).ToString();

			if (category == "print"
			    || category == "log"
			    || category == "info") { Debug.Log(message); }
			else if (category == "debug") { Debug.Log($"{message}\n{callstack}"); }
			else if (category == "warning") {
				string output = $"{message}";
				Debug.LogWarning($"{output}\n{callstack}");
			}
			else if (category == "error" || category == "userError") {
				string output = $"{message}";
				Debug.LogError($"{output}\n{callstack}");
			}
		}

		private static void HandlePlatformsSetup()
		{
			if(Application.platform == RuntimePlatform.IPhonePlayer)
			{
				CollabPaths.ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Caches/ProgramData");

				CollabPaths.DownloadCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Caches/CacheV1");

				CollabPaths.ContentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Documents/{BuildInfo.applicationPathName}Content");

				CollabPaths.ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Documents/{BuildInfo.applicationPathName}Pictures");

				CollabPaths.TempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/tmp");

				CollabPaths.SecondaryApplicationsPath = CollabPaths.ProgramDataPath;
			}
#if UNITY_2022
			if (Application.platform == RuntimePlatform.VisionOS /*TODO: Does this use the same settings as iOS?*/)
			{
				CollabPaths.ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Caches/ProgramData");

				CollabPaths.DownloadCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Caches/CacheV1");

				CollabPaths.ContentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Documents/{BuildInfo.applicationPathName}Content");

				CollabPaths.ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Documents/{BuildInfo.applicationPathName}Pictures");

				CollabPaths.TempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/tmp");

				CollabPaths.SecondaryApplicationsPath = CollabPaths.ProgramDataPath;
			}
#endif
			else if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				var ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/" + BuildInfo.AppNameStorageFolder);

				CollabPaths.ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/" + BuildInfo.AppNameStorageFolder);

				CollabPaths.DownloadCachePath = Path.Combine(ProgramDataPath, "CacheV1");

				CollabPaths.ContentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BuildInfo.applicationPathName);

				CollabPaths.ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), BuildInfo.applicationPathName);

				CollabPaths.TempPath = Path.Combine(ProgramDataPath, "tmp");

				CollabPaths.SecondaryApplicationsPath = ProgramDataPath;
			}
			else if(Application.platform == RuntimePlatform.Android)
			{
				var DataPathRoot = Application.persistentDataPath;

				CollabPaths.ProgramDataPath = $"{DataPathRoot}Data";

				CollabPaths.DownloadCachePath = $"{DataPathRoot}CacheV1";

				CollabPaths.ContentPath = $"{DataPathRoot}Documents";

				CollabPaths.ScreenshotsPath = $"{DataPathRoot}Pictures";

				CollabPaths.TempPath = $"{DataPathRoot}tmp";

				CollabPaths.SecondaryApplicationsPath = CollabPaths.ProgramDataPath;
			}
			else if(Application.platform == RuntimePlatform.WebGLPlayer)
			{
				CollabPaths.ProgramDataPath = Application.persistentDataPath;

				CollabPaths.DownloadCachePath = Path.Combine(Application.persistentDataPath, "CacheV1");

				CollabPaths.TempPath = Path.Combine(Application.persistentDataPath, "tmp");

				// TODO REMOVE IRRELEVANT PATHS
				CollabPaths.ContentPath = Application.persistentDataPath;

				CollabPaths.ScreenshotsPath = Application.persistentDataPath;
			}
			//TODO: DOES MAGIC LEAP JUST WORK WITH ANDROID PATHS?  WE SHOULD CHECK.
		}
	}
}