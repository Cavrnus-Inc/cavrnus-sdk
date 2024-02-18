using Collab.Base.Collections;
using Collab.LiveRoomSystem;
using Collab.Proxy.LiveJournal;
using Collab.Proxy.Comm.LocalTypes;
using System;
using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk;
using System.Linq;
using CavrnusSdk.Setup;

namespace CavrnusCore
{
	internal class CavrnusObjectCreationHandler : IDisposable
	{
		private List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs;

        private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

		private IDisposable disp;

		private CavrnusSpaceConnection spaceConn;

		public CavrnusObjectCreationHandler(List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs, CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;
			this.spawnablePrefabs = spawnablePrefabs;

			var creationHandler = spaceConn.RoomSystem.LiveJournal.GetObjectCreationReciever();

			var visibleObjectOps = creationHandler.GetMultiEntryWatcher<OpCreateObjectLive>().VisibleOps;

			disp = visibleObjectOps.BindAll(ObjectCreated, ObjectRemoved);
		}

		public void Dispose() { disp.Dispose(); }

		internal void ObjectCreated(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createOp.Op.ObjectType is ContentTypeWellKnownId cId) 
			{
				if (spawnablePrefabs.Any(sp => sp.UniqueId == cId.WellKnownId)) 
				{
					var prefab = spawnablePrefabs.FirstOrDefault(sp => sp.UniqueId == cId.WellKnownId)?.Object;
                    var ob = GameObject.Instantiate(prefab);
					createdObjects[createOp.Op.NewObjectId] = ob.gameObject;
					ob.gameObject.name = $"{createOp.Op.NewObjectId} ({prefab.name})";
					ob.gameObject.AddComponent<CavrnusSpawnedObjectFlag>().Init(new CavrnusSpawnedObject(createOp.Op.NewObjectId, createOp.Id, spaceConn));
					CavrnusPropertyHelpers.ResetLiveHierarchyRootName(ob.gameObject, createOp.Op.NewObjectId);
				}
				else {
					Debug.LogWarning(
						$"Could not find spawnable prefab with ID {cId.WellKnownId} in the \"Cavrnus Spatial Connector\"");
				}
			}
			else if(createOp.Op.ObjectType is ContentTypeId ctId)
			{
				if(!spawnablePrefabs.Any((sp => sp.UniqueId == "HoloLoader")))
				{
					//TODO: CHECK FILE TYPE FOR COMPATABILITY!!!!!!!!!!!!!!!
					Debug.LogWarning("Cannot load file since HoloLoader is not present in Spawnable Prefabs");
					return;
				}

				var prefab = spawnablePrefabs.FirstOrDefault(sp => sp.UniqueId == "HoloLoader")?.Object;
				var ob = GameObject.Instantiate(prefab);
				createdObjects[createOp.Op.NewObjectId] = ob.gameObject;
				ob.gameObject.name = $"{createOp.Op.NewObjectId} ({prefab.name})";
				ob.gameObject.AddComponent<CavrnusSpawnedObjectFlag>().Init(new CavrnusSpawnedObject(createOp.Op.NewObjectId, createOp.Id, spaceConn));
				CavrnusPropertyHelpers.ResetLiveHierarchyRootName(ob.gameObject, createOp.Op.NewObjectId);
			}
			else if (createOp.Op.ObjectType is ContentTypeUrl cUrl) {
				Debug.LogWarning($"ContentType URL coming soon...");
			}
			else {
				Debug.LogWarning($"ContentType {createOp.Op.ObjectType} is not currently supported by the Cavrnus SDK.");
			}
		}

		internal void ObjectRemoved(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createdObjects.ContainsKey(createOp.Op.NewObjectId)) {
				GameObject.Destroy(createdObjects[createOp.Op.NewObjectId]);
				createdObjects.Remove(createOp.Op.NewObjectId);
			}
		}
	}
}