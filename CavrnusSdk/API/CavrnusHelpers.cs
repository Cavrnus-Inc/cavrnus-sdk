using Assets.Scripts;
using Collab.Base.Core;
using Collab.Proxy;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Collab.Proxy.Comm.RestApi;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Collab.Proxy.Comm;
using System.Linq;
using Collab.LiveRoomSystem;
using Collab.LiveRoomSystem.LiveObjectManagement;
using Collab.Proxy.Comm.NotifyApi;
using Collab.Proxy.Content;
using Collab.RtcCommon;
using Collab.SystemProxy;
using Collab.Base.Collections;
using System;
using Collab.Proxy.Comm.LocalTypes;

namespace CavrnusSdk
{
	public static class CavrnusHelpers
	{
		public static UnityScheduler Scheduler{
			get {
				if (scheduler == null) {
					scheduler = new GameObject("Scheduler").AddComponent<UnityScheduler>();
					scheduler.Setup();
				}

				return scheduler;
			}
		}

		private static UnityScheduler scheduler = null;

		public static INetworkRequestImplementation NetworkRequestImpl;
		public static IRtcContext RtcContext;

		public static void Setup()
		{
			UnityBase.HelperFunctions.MainThread = Thread.CurrentThread;
			DebugOutput.MessageEvent += DoRecvMessage;
			CollabPaths.FlushTemporaryFilePath();

			NetworkRequestImpl = new FrameworkNetworkRequestImplementation();



			IRtcSystem rtcSystem;

			if (CavrnusCore.Instance.CavrnusSettings.DisableVoiceAndVideo) 
			{ 
				rtcSystem = new RtcSystemUnavailable(); 
			}
			else
			{
				rtcSystem = new RtcSystemUnity(Scheduler, CavrnusCore.Instance.CavrnusSettings.BuildingForMagicLeap);
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

		/// <summary>
		/// Attempts to log into Cavrnus using the given server, email, and password
		/// </summary>
		/// <param name="server"></param>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static async Task Authenticate(string server, string email, string password)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server);

			RestUserCommunication ruc =
				new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.LoginRequest req = new RestUserCommunication.LoginRequest();
			req.email = email;
			req.password = password;
			var token = await ruc.PostLocalAccountLoginAsync(req);

			DebugOutput.Info("Logged in, token: " + token.token);

			CavrnusAuth.Endpoint = endpoint.WithAuthorization(token.token);
		}

		public static async Task JoinLinkAsync(string link, string joinWithName)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(link);

			RestUserCommunication ruc =
				new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());

			var url = new Uri(link);
			var friendlyId = url.Segments[url.Segments.Length - 1];

			var info = await ruc.GetInstantLinkInfo(friendlyId);

			RestUserCommunication.GuestLoginRequest req =
				new RestUserCommunication.GuestLoginRequest() {screenName = joinWithName, ssoToken = "",};

			var token = await ruc.PostInstantLinkLoginAsync(info.id, req);

			DebugOutput.Info("Logged in, token: " + token.token);
			CavrnusAuth.Endpoint = endpoint.WithAuthorization(token.token);

			await JoinSpaceFromId(info.room.id);
		}

		public static async Task<List<CavrnusSpaceInfo>> GetAllAvailableSpaces()
		{
			RestRoomCommunication rrc =
				new RestRoomCommunication(CavrnusAuth.Endpoint, new FrameworkNetworkRequestImplementation());

			var uri = await rrc.GetUserFullRoomsAndInvitesInfoAsync();

			DebugOutput.Info($"Fetched rooms list: {uri.rooms.Length} rooms.");

			var res = new List<CavrnusSpaceInfo>();
			foreach (var room in uri.rooms) res.Add(new CavrnusSpaceInfo(room));

			res.Sort((x, y) => DateTime.Compare(x.LastAccessedTime, y.LastAccessedTime));

			return res;
		}

		public static async Task<string> CreateSpace(string newSpaceName)
		{
			RestRoomCommunication rrc =
				new RestRoomCommunication(CavrnusAuth.Endpoint, new FrameworkNetworkRequestImplementation());

			RestRoomCommunication.CreateRoomRequest cr = new RestRoomCommunication.CreateRoomRequest();
			cr.name = newSpaceName;
			var id = await rrc.PostCreateRoomAsync(cr);
			return id._id;
		}

		public static ISetting<RtcInputSource> DesiredVideoStream = new Setting<RtcInputSource>(null);

		public static async Task<CavrnusSpaceConnection> JoinSpaceFromId(string roomId)
		{
			//var engineConnector = new CavrnusUnityConnector(Scheduler);

			//if (!useSyncedContent)
			//	engineConnector = null;

			var contentManager = new ServerContentCacheManager(new FrameworkNetworkRequestImplementation());
			contentManager.SetEndpoint(CavrnusAuth.Endpoint);

			var notify = new NotifyCommunication(() => new NotifyWebsocket(Scheduler.BaseScheduler),
			                                     Scheduler.BaseScheduler);
			notify.Initialize(CavrnusAuth.Endpoint, true);

			LiveObjectManagementContext liveObjectContext = new LiveObjectManagementContext() {
				EngineConnector = null, //engineConnector,
				Notify = notify,
				Scheduler = Scheduler.BaseScheduler,
				ServerContentManager = contentManager,
			};

			RoomSystem rs = new RoomSystem(RtcContext, Scheduler.BaseScheduler, liveObjectContext, null, null, null,
			                               null);

			rs.InitializeConnection(CavrnusAuth.Endpoint, roomId);

			await rs.AwaitJournalProcessed();

			await rs.AwaitLocalUser();

			rs.Comm.LocalCommUser.Value.SetupVideoSources(DesiredVideoStream, DesiredVideoStream);

			var connection = new CavrnusSpaceConnection(rs);
			CavrnusSpaceJoinEvent.InvokeSpaceJoin(connection);

			Scheduler.ExecOnApplicationQuit(() => connection.Dispose());

			return connection;
		}

		public static string PostSpawnObjectWithUniqueId(CavrnusSpaceConnection spaceConn, string uniqueId,
		                                                 CavrnusPropertyHelpers.TransformData pos = null)
		{
			var prefabToUse =
				CavrnusCore.Instance.SpawnablePrefabs.SpawnablePrefabs.FirstOrDefault(
					sp => sp.UniqueIdentifier == uniqueId);

			if (prefabToUse == null) {
				Debug.LogError(
					$"Attempting to spawn prefab with unique ID {uniqueId}, but it does not exist in \"Assets/CavrnusSdk/Cavrnus Spawnable Prefabs Lookup\".");
				return null;
			}

			var newId = spaceConn.RoomSystem.Comm.CreateNewUniqueObjectId();
			var creatorId = spaceConn.RoomSystem.Comm.LocalCommUser.Value.ConnectionId;
			var contentType = new ContentTypeWellKnownId(uniqueId);

			var createOp = new OpCreateObjectLive(null, newId, creatorId, contentType);

			spaceConn.RoomSystem.Comm.SendJournalEntry(createOp.ToOp(), null);

			if (pos != null) {
				if (prefabToUse.GetComponent<SyncTransform>() == null) {
					Debug.LogError(
						$"Attempting to set the Transform of spawned prefab with unique Id {uniqueId}, but it has no SyncTransform component.");
					return newId;
				}

				var containerPath = prefabToUse.GetComponent<CavrnusPropertiesContainer>().UniqueContainerPath.ToList();
				containerPath.Insert(0, newId);

				CavrnusPropertyHelpers.UpdateTransformProperty(spaceConn, containerPath.ToArray(),
				                                                   prefabToUse.GetComponent<SyncTransform>()
				                                                              .PropertyName, pos.LocalPosition,
				                                                   pos.LocalEulerAngles, pos.LocalScale);
			}

			return newId;
		}

		public static void DeleteSpawnedObject(CavrnusSpaceConnection spaceConn, CavrnusSpawnedObject spawnedOb)
		{
			OperationIdLive rootOpId = new OperationIdLive(spawnedOb.CreationOpId);

			var singles = new List<string> {rootOpId.Id};

			var deleteOp = new OpRemoveOpsLive(OpRemoveOpsLive.RemovalTypes.None) {OpsToRemove = singles};

			spaceConn.RoomSystem.Comm.SendJournalEntry(deleteOp.ToOp(), null);
		}
	}
}