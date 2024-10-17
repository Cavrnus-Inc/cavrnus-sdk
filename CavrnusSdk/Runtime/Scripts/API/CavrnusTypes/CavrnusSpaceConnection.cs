using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CavrnusSdk.Setup;
using Collab.Base.Collections;
using Collab.LiveRoomSystem;
using Collab.RtcCommon;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceConnection : IDisposable
	{
		public CavrnusSpaceConnectionConfig Config{ get; private set; }

		internal IReadonlySetting<CavrnusSpaceConnectionData> CurrentSpaceConnection => currentSpaceConnection;
		private readonly ISetting<CavrnusSpaceConnectionData> currentSpaceConnection = new Setting<CavrnusSpaceConnectionData>();
		
		internal IReadonlySetting<CavrnusUser> CurrentLocalUserSetting => currentLocalUserSetting;
		private readonly ISetting<CavrnusUser> currentLocalUserSetting = new Setting<CavrnusUser>();
		
		internal IReadonlySetting<IRtcContext> CurrentRtcContext => currentRtcContext;
		private readonly ISetting<IRtcContext> currentRtcContext = new Setting<IRtcContext>();
		
		private readonly List<Action<string>> onLoadingEvents = new();
		private readonly NotifyList<Action<CavrnusSpaceConnection>> onConnectedEvents = new();

		private readonly List<IDisposable> bindings = new ();
		
		public CavrnusSpaceConnection(CavrnusSpaceConnectionConfig config)
		{
			Config = config;
			
			bindings.Add(CurrentSpaceConnection.Bind(sc =>
			{
				if (sc == null) 
					return;
				
				if (onConnectedEvents.Count > 0)
					onConnectedEvents.ForEach(callback => callback?.Invoke(this));

				onConnectedEvents.Clear();
				
				_ = GetLocalUserAsync(sc);
			}));
		}	
		
		internal void Update(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, CavrnusSpaceConnectionConfig config)
		{
			Config = config;
			currentSpaceConnection.Value?.Dispose();
			
			currentSpaceConnection.Value = new CavrnusSpaceConnectionData(roomSystem, spawnableObjects, this);
			currentRtcContext.Value = roomSystem.RtcContext;
		}
		
		private async Task GetLocalUserAsync(CavrnusSpaceConnectionData scd)
		{
			try {
				var user = await scd.RoomSystem.AwaitLocalUser();

				if (currentLocalUserSetting.Value == null)
					currentLocalUserSetting.Value = new CavrnusUser(user, this);
				else
					currentLocalUserSetting.Value.InitUser(user);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		
		internal IDisposable AwaitLocalUser(Action<CavrnusUser> onLocalUser)
		{
			return currentLocalUserSetting.BindUntilTrue(lu => {
				if (lu == null)
					return false;

				onLocalUser?.Invoke(lu);
				return true;
			});
		}

		internal IDisposable BindLocalUser(Action<CavrnusUser> onLocalUser)
		{
			return currentLocalUserSetting.Bind(lu => {
				if (lu == null)
					return;
				onLocalUser?.Invoke(lu);
			});
		}

		internal void DoLoadingEvents(string joinId)
		{
			onLoadingEvents.ForEach(le => le?.Invoke(joinId));
		}

		internal void TrackLoadingEvent(Action<string> onLoading)
		{
			onLoadingEvents.Add(onLoading);
		}

		internal void TrackConnectedEvent(Action<CavrnusSpaceConnection> onConnected)
		{
			if (currentSpaceConnection.Value == null)
				onConnectedEvents.Add(onConnected);
			else
				onConnected?.Invoke(this);
		}

		internal IDisposable BindConnection(Action<CavrnusSpaceConnection> onConnection)
		{
			return currentSpaceConnection.Bind((c) =>
			{
				if (c != null) onConnection(this);
			});
		}
		
		public void Dispose()
		{
			currentSpaceConnection?.Value?.Dispose();
			bindings?.ForEach(b => b?.Dispose());
		}
	}
}