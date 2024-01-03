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
		public static NotifyCommunication Notify { get; private set; }
		public static LivePolicyEvaluator LivePolicyEvaluator { get; private set; }

		public static MultiTranslatingSetting<bool, bool, bool> NotifyPoliciesAndRolesFullyLoaded{ get; set; }

		public static INetworkRequestImplementation NetworkRequestImpl;
		public static IRtcContext RtcContext;

		public static void Setup()
		{
			UnityBase.HelperFunctions.MainThread = Thread.CurrentThread;
			DebugOutput.MessageEvent += DoRecvMessage;
			CollabPaths.FlushTemporaryFilePath();

			NetworkRequestImpl = new FrameworkNetworkRequestImplementation();

			Notify = new NotifyCommunication(() => new NotifyWebsocket(Scheduler.BaseScheduler), Scheduler.BaseScheduler);
			LivePolicyEvaluator = new LivePolicyEvaluator(Notify.PoliciesSystem.AllPolicies, Notify.PoliciesSystem.IsActive);
			
			IRtcSystem rtcSystem;
			
			if (CavrnusCore.Instance.CavrnusSettings.DisableVoiceAndVideo) 
			{ 
				rtcSystem = new RtcSystemUnavailable(); 
			}
			else
			{
				rtcSystem = new RtcSystemUnity(Scheduler, CavrnusCore.Instance.CavrnusSettings.DisableAcousticEchoCancellation);
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
			
			NotifySetup();
		}

		private static void NotifySetup()
		{
			Notify.Initialize(CavrnusAuth.Endpoint, true);
			Notify.ObjectsSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			Notify.PoliciesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			Notify.RolesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			Notify.UsersSystem.StartListening(null, err => DebugOutput.Error(err.ToString()));
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

			NotifySetup();

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

			Notify.RoomsSystem.StartListeningSpecificAsync(roomId);

			var contentManager = new ServerContentCacheManager(new FrameworkNetworkRequestImplementation());
			contentManager.SetEndpoint(CavrnusAuth.Endpoint);

			LiveObjectManagementContext liveObjectContext = new LiveObjectManagementContext() {
				EngineConnector = null, //engineConnector,
				Notify = Notify,
				Scheduler = Scheduler.BaseScheduler,
				ServerContentManager = contentManager,
			};

			var env = new RoomSystemEnvironment() {
				PolicyEvaluator = LivePolicyEvaluator,
				RolesMaintainer = Notify.ContextualRoles,
			};

			RoomSystem rs = new RoomSystem(RtcContext, Scheduler.BaseScheduler, liveObjectContext, null, null, env, null);

			rs.InitializeConnection(CavrnusAuth.Endpoint, roomId);

			await rs.AwaitJournalProcessed();

			await rs.AwaitLocalUser();

			rs.Comm.LocalCommUser.Value.SetupVideoSources(DesiredVideoStream, DesiredVideoStream);

			var connection = new CavrnusSpaceConnection(rs);
			CavrnusSpaceJoinEvent.InvokeSpaceJoin(connection);

			Scheduler.ExecOnApplicationQuit(() => connection.Dispose());

			return connection;
		}

		public static void DeleteSpawnedObject(CavrnusSpaceConnection spaceConn, CavrnusSpawnedObject spawnedOb)
		{
			OperationIdLive rootOpId = new OperationIdLive(spawnedOb.CreationOpId);

			var singles = new List<string> {rootOpId.Id};

			var deleteOp = new OpRemoveOpsLive(OpRemoveOpsLive.RemovalTypes.None) {OpsToRemove = singles};

			spaceConn.RoomSystem.Comm.SendJournalEntry(deleteOp.ToOp(), null);
		}

		public static IReadonlySetting<ISessionCommunicationLocalUser> GetLocalUser(CavrnusSpaceConnection csc)
		{
			return csc.RoomSystem.Comm.LocalCommUser;
		}
		
	}
}