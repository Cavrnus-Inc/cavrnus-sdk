using System;
using System.Collections.Generic;
using Collab.LiveRoomSystem;
using CavrnusCore;
using CavrnusSdk.Setup;
using Collab.Base.Collections;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk.API
{
    internal class CavrnusSpaceConnectionData : IDisposable
    {
        public readonly RoomSystem RoomSystem;
        public readonly CavrnusObjectCreationHandler CreationHandler;
        private readonly IDisposable timeUpdater;
        
        public CavrnusSpaceConnectionData(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, CavrnusSpaceConnection sc)
        {
            RoomSystem = roomSystem;
            CreationHandler = new CavrnusObjectCreationHandler(spawnableObjects, sc);
            timeUpdater = CavrnusStatics.Scheduler.ExecInMainThreadRepeatingEachFrame(() => RoomSystem.DateTimeProperties.Update(Time.realtimeSinceStartupAsDouble));
        }

        public void Dispose()
        {
            RoomSystem?.Shutdown();
            CreationHandler?.Dispose();
            timeUpdater?.Dispose();
        }
    }
}