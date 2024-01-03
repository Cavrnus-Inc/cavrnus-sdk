using System;
using Collab.LiveRoomSystem;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusSpaceConnection : IDisposable
	{
		public RoomSystem RoomSystem;
		public CavrnusSpaceUsersList UsersList;
		private CavrnusObjectCreationHandler CreationHandler;
		private IDisposable timeUpdater;

		public CavrnusSpaceConnection(RoomSystem roomSystem)
		{
			RoomSystem = roomSystem;
			UsersList = new CavrnusSpaceUsersList(roomSystem);
			CreationHandler = new CavrnusObjectCreationHandler(RoomSystem);

			timeUpdater = HelperFunctions.ExecInMainThreadRepeatingEachFrame(CavrnusHelpers.Scheduler,
				() => RoomSystem.DateTimeProperties.Update(Time.realtimeSinceStartupAsDouble));
		}

		public void Dispose()
		{
			RoomSystem?.Shutdown();
			UsersList?.Shutdown();
			CreationHandler?.Dispose();
			timeUpdater?.Dispose();
		}
	}
}