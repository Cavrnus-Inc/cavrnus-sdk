using System;
using Collab.LiveRoomSystem;

namespace CavrnusSdk
{
	public class CavrnusSpaceConnection : IDisposable
	{
		public RoomSystem RoomSystem;
		public CavrnusSpaceUsersList UsersList;
		private CavrnusObjectCreationHandler CreationHandler;

		public CavrnusSpaceConnection(RoomSystem roomSystem)
		{
			RoomSystem = roomSystem;
			UsersList = new CavrnusSpaceUsersList(roomSystem);
			CreationHandler = new CavrnusObjectCreationHandler(RoomSystem);
		}

		public void Dispose()
		{
			RoomSystem?.Shutdown();
			UsersList?.Shutdown();
			CreationHandler?.Dispose();
		}
	}
}