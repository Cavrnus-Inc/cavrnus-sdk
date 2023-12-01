using System;
using System.IO;
using Collab.Base.Core;
using Collab.Base.Scheduler;
using Collab.Proxy;
using UnityBase;
using Collab.SystemProxy;

namespace CavrnusSdk
{
	public class SystemProxyManagerScript
	{
		private IFileThumbnailRequestor fileThumbnailRequestor{ get; set; } = new NilFileThumbnailRequestor();
		private SystemProxyClientCommon systemProxyClient;
		public SystemProxyClientCommon SystemProxyClient => systemProxyClient;

		public ClientDebugReceiver ClientDebugReceiver{ get; private set; }

		public SystemProxyManagerScript(IScheduler scheduler)
		{
			systemProxyClient = null;
			string remoteProxyPath = Path.Combine(
				Path.Combine(UserSettings.Instance.ApplicationStreamingAssetsPath, "SystemProxy"),
				"CavrnusCommunicationSystem.exe");

			SystemProxyClient systemProxyClientExe;
			if (!File.Exists(remoteProxyPath)) {
				string dataPath = Path.Combine(CollabPaths.SecondaryApplicationsPath, "SystemProxy",
				                               BuildInfo.buildType + "_" + BuildInfo.buildVersion);
				remoteProxyPath = Path.Combine(dataPath, "CavrnusCommunicationSystem.exe");
				if (!File.Exists(remoteProxyPath)) {
					string remoteProxyUnpackZip =
						Path.Combine(UserSettings.Instance.ApplicationStreamingAssetsPath, "SystemProxy.zip");
					if (File.Exists(remoteProxyUnpackZip)) {
						FileHelpers.EnsureDirectoryExists(dataPath);
						ZipHelper.UnZipTo(remoteProxyUnpackZip, dataPath);
					}
				}
			}

			if (File.Exists(remoteProxyPath)) {
				systemProxyClientExe = new SystemProxyClient(remoteProxyPath, scheduler);

				var proxyLogPath = Path.Combine(CollabPaths.ProgramDataPath, "RtcLogs");
				if (!Directory.Exists(proxyLogPath)) Directory.CreateDirectory(proxyLogPath);
				var importLogNextFile = Path.Combine(proxyLogPath,
				                                     "rtclog_" +
				                                     DateTime.Now.ToLocalTime().ToString("yyyyMMdd_HHmmss") + ".txt");
				systemProxyClientExe.SetLogPath(importLogNextFile);
				systemProxyClient = systemProxyClientExe;
			}
			else { DebugOutput.Warning("No System Proxy found. RTC will be unavailable."); }

			ClientDebugReceiver = new ClientDebugReceiver(systemProxyClient);
			//ClientDebugReceiver.DebugMessageEvent += (type, msg) => DebugOutput.Out(type, "Remote: " + msg);

			fileThumbnailRequestor = new FileThumbnailRequestor(systemProxyClient);

			if (systemProxyClient != null) {
				systemProxyClient.Initialize();
				systemProxyClient.ProxyRestartingEvent += () => DebugOutput.Log("System Proxy is restarting.");
				systemProxyClient.ProxyFailedEvent += () =>
					DebugOutput.Error("Communications System has failed. Voice communications will no longer work.");
			}
		}

		void OnApplicationQuit()
		{
			if (fileThumbnailRequestor != null) fileThumbnailRequestor.Shutdown();
			fileThumbnailRequestor = null;

			if (ClientDebugReceiver != null) ClientDebugReceiver.Shutdown();
			ClientDebugReceiver = null;

			if (systemProxyClient != null) { systemProxyClient.Shutdown(); }

			systemProxyClient = null;
		}
	}
}