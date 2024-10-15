using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CavrnusSdk.Setup;
using Collab.Base.Collections;
using Collab.LiveRoomSystem;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceConnection : IDisposable
	{
		public CavrnusSpaceConnectionConfig Config{ get; private set; }

		internal IReadonlySetting<CavrnusSpaceConnectionData> CurrentSpaceConnection => currentSpaceConnection;
		private readonly ISetting<CavrnusSpaceConnectionData> currentSpaceConnection = new Setting<CavrnusSpaceConnectionData>();
		
		internal IReadonlySetting<CavrnusUser> CurrentLocalUserSetting => currentLocalUserSetting;
		private ISetting<CavrnusUser> currentLocalUserSetting = new Setting<CavrnusUser>();
		
		private readonly List<Action<string>> onLoadingEvents = new();
		private readonly NotifyList<Action<CavrnusSpaceConnection>> onConnectedEvents = new();

		private readonly List<IDisposable> bindings = new ();
		
		public CavrnusSpaceConnection(CavrnusSpaceConnectionConfig config)
		{
			Config = config;
			
			bindings.Add(CurrentSpaceConnection.Bind(sc => {
				if (onConnectedEvents.Count > 0)
					onConnectedEvents.ForEach(callback => callback?.Invoke(this));

				_= GetLocalUserAsync();
				
				onConnectedEvents.Clear();
			}));
		}		
		
		private async Task GetLocalUserAsync()
		{
			try {
				var user = await currentSpaceConnection.Value.RoomSystem.AwaitLocalUser();
				currentLocalUserSetting.Value = new CavrnusUser(user, this);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		
		internal void AwaitLocalUser(Action<CavrnusUser> localUser)
		{
			bindings.Add(currentLocalUserSetting.Bind(lu => {
				if (lu == null)
					return;
				
				localUser?.Invoke(lu);
			}));
		}
		
		internal void Update(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, CavrnusSpaceConnectionConfig config)
		{
			Config = config;
			currentSpaceConnection.Value?.Dispose(); // exit space before rejoining
			
			currentSpaceConnection.Value = new CavrnusSpaceConnectionData(roomSystem, spawnableObjects, this);
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