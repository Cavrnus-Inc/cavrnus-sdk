using Collab.Base.Core;
using Collab.Proxy.Comm.RestApi;
using Collab.Proxy.Comm;
using System.Collections.Generic;
using System;
using Collab.LiveRoomSystem.LiveObjectManagement;
using Collab.LiveRoomSystem;
using Collab.Proxy.Content;
using Collab.Base.Collections;
using Collab.Proxy.Comm.NotifyApi;
using Collab.Proxy.Comm.LiveTypes;
using CavrnusSdk.API;
using CavrnusSdk.Setup;

namespace CavrnusCore
{
	internal static class CavrnusSpaceHelpers
	{
		internal static List<CavrnusSpaceConnection> SpaceConnections = new List<CavrnusSpaceConnection>();

		internal static async void JoinSpace(string joinId, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, Action<CavrnusSpaceConnection> onConnected, Action<string> onFailure)
		{
			if (onLoadingEvents.Count > 0)
			{
				foreach (var onLoadingEvent in onLoadingEvents)
					onLoadingEvent?.Invoke(joinId);
				onLoadingEvents.Clear();
			}

			var contentManager = new ServerContentCacheManager(new FrameworkNetworkRequestImplementation());
			contentManager.SetEndpoint(CavrnusAuthHelpers.CurrentAuthentication.Endpoint);

			var rsOptions = new RoomSystemOptions {
				CopresenceToProperties = false,
				LoadObjectsChats = false,
				LoadContentAssetsMetadata = false,
				LoadContentAssetsTextures = false,
				LoadContentObjectsMetadata = true,
				LoadContentObjectsScripts = true,
				LoadObjectsArTrackers = false,
				LoadContentObjects2DContent = false,
				LoadContentObjectsHoloContent = false,
				TranslationLanguage = null
			};

			var env = new RoomSystemEnvironment()
			{
				PolicyEvaluator = CavrnusStatics.LivePolicyEvaluator,
				RolesMaintainer = CavrnusStatics.Notify.ContextualRoles,
				Scheduler = CavrnusStatics.Scheduler.BaseScheduler
			};

			RoomSystem rs = new RoomSystem(CavrnusStatics.RtcContext, env, rsOptions);

			rs.InitializeConnection(CavrnusAuthHelpers.CurrentAuthentication.Endpoint, joinId);

			await rs.AwaitJournalProcessed();

			var lu = await rs.AwaitLocalUser();

			rs.Comm.LocalCommUser.Value.SetupVideoSources(CavrnusStatics.DesiredVideoStream, CavrnusStatics.DesiredVideoStream);

			var connection = new CavrnusSpaceConnection(rs, spawnableObjects);

			SpaceConnections.Add(connection);

			if(onConnectedEvents.Count > 0)
			{
				foreach (var onConnectedEvent in onConnectedEvents)
					onConnectedEvent?.Invoke(connection);
				onConnectedEvents.Clear();
			}

			onConnected(connection);
		}

		private static List<Action<string>> onLoadingEvents = new List<Action<string>>();

		internal static void AwaitAnySpaceBeginLoading(Action<string> onLoading)
		{
			onLoadingEvents.Add(onLoading);
		}

		private static List<Action<CavrnusSpaceConnection>> onConnectedEvents = new List<Action<CavrnusSpaceConnection>>();

		internal static void AwaitAnySpaceConnection(Action<CavrnusSpaceConnection> onConnected)
		{
			if(SpaceConnections.Count > 0)
			{
				onConnected(SpaceConnections[0]);
			}
			else
			{
				onConnectedEvents.Add(onConnected);
			}
		}

		internal static async void GetCurrentlyAvailableSpaces(Action<List<CavrnusSpaceInfo>> onRecvCurrentJoinableSpaces)
		{
			RestRoomCommunication rrc =
				new RestRoomCommunication(CavrnusAuthHelpers.CurrentAuthentication.Endpoint, new FrameworkNetworkRequestImplementation());

			var uri = await rrc.GetUserFullRoomsAndInvitesInfoAsync();

			DebugOutput.Info($"Fetched rooms list: {uri.rooms.Length} rooms.");

			var res = new List<CavrnusSpaceInfo>();
			foreach (var room in uri.rooms) res.Add(new CavrnusSpaceInfo(room.name, room._id, room.modifiedAt, room.thumbnailContentUrl));

			res.Sort((x, y) => DateTime.Compare(x.LastAccessedTime, y.LastAccessedTime));

			onRecvCurrentJoinableSpaces(res);
		}

		internal static IDisposable BindAllAvailableSpaces(Action<CavrnusSpaceInfo> spaceAdded, Action<CavrnusSpaceInfo> spaceUpdated, Action<CavrnusSpaceInfo> spaceRemoved)
		{
			List<IDisposable> disposables = new List<IDisposable>();

			CavrnusStatics.Notify.RoomsSystem.StartListeningAsync();

			var accessFilter = new NotifyDictionaryFiltererDynamic<string, INotifyDataRoom>(CavrnusStatics.Notify.RoomsSystem.RoomsInfo,
				(id, ndr) =>
				{
					return ndr.Access.Value != RoomMetadata.Types.RoomAccess.None && ndr.ConnectedMember.Value != null;
				},
				(id, ndr, hook) => new IDisposable[] { ndr.Access.Hook(hook), ndr.ConnectedMember.Hook(hook) });
			disposables.Add(accessFilter);

			#region Room Type Filter
			var typeFilter = new NotifyDictionaryFilterer<string, INotifyDataRoom>(accessFilter.Result,
				(id, ndr) => ndr.RoomType != "instance");
			disposables.Add(typeFilter);
			#endregion

			var archivedFilter = new NotifyDictionaryFiltererDynamic<string, INotifyDataRoom>(typeFilter.Result,
				(s, ndr) =>
				{
					bool visible = !(ndr.ConnectedMember.Value?.Hidden?.Value ?? false) &&
								   ndr.Access.Value != RoomMetadata.Types.RoomAccess.None;
					return visible;
				},
			(s, entry, hook) => new IDisposable[] { entry.ConnectedMember.Value?.Hidden?.Hook(hook), entry.Access.Hook(hook), });
			disposables.Add(archivedFilter);

			NotifyDictionaryListMapper<string, INotifyDataRoom, CavrnusSpaceInfo> mapper =
				new NotifyDictionaryListMapper<string, INotifyDataRoom, CavrnusSpaceInfo>(archivedFilter.Result,
					(s, ile) => new CavrnusSpaceInfo(ile.Name.Value, ile.Id, ile.ConnectedMember.Value.LastAccess.Value.Value, ile.ThumbnailUrl.Value.ToString()),
					(a, b) => Comparer<DateTime?>.Default.Compare(b.LastAccessedTime, a.LastAccessedTime));
			disposables.Add(mapper);

			var bnd = mapper.Result.BindAll(spaceAdded, spaceRemoved);
			disposables.Add(bnd);

			return new MultiDisposalHelper(disposables.ToArray());
		}
	}
}