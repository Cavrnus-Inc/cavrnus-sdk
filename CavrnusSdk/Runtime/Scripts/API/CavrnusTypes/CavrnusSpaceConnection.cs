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
			
			bindings.Add(CurrentSpaceConnection.Bind(sc => {
				if (sc == null) 
					return;
				
				if (onConnectedEvents.Count > 0)
					onConnectedEvents.ForEach(callback => callback?.Invoke(this));

				onConnectedEvents.Clear();
				
				_= GetLocalUserAsync(sc);
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
		
		internal void AwaitLocalUser(Action<CavrnusUser> localUser)
		{
			IDisposable binding = null;
			binding = currentLocalUserSetting.Bind(lu => {
				if (lu == null)
					return;

				localUser?.Invoke(lu);
				binding?.Dispose();
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
		
		public void Dispose()
		{
			currentSpaceConnection?.Value?.Dispose();
			bindings?.ForEach(b => b?.Dispose());
		}
	}
}