using Collab.LiveRoomSystem;
using System;
using UnityBase;
using UnityEngine;
using CavrnusCore;
using CavrnusSdk.Setup;
using System.Collections.Generic;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceConnection : IDisposable
	{
		public RoomSystem RoomSystem;
		public CavrnusObjectCreationHandler CreationHandler;
		private IDisposable timeUpdater;

		public CavrnusSpaceConnection(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects)
		{
			RoomSystem = roomSystem;
			CreationHandler = new CavrnusObjectCreationHandler(spawnableObjects, this);

			timeUpdater = HelperFunctions.ExecInMainThreadRepeatingEachFrame(CavrnusStatics.Scheduler,
				() => RoomSystem.DateTimeProperties.Update(Time.realtimeSinceStartupAsDouble));
		}

		public void Dispose()
		{
			RoomSystem?.Shutdown();
			CreationHandler?.Dispose();
			timeUpdater?.Dispose();
		}
	}
}