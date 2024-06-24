using Collab.Base.Collections;
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
	public class CavrnusObjectCreationHandler : IDisposable
	{
		public Dictionary<string, Action<CavrnusSpawnedObject, GameObject>> SpawnCallbacks = new Dictionary<string, Action<CavrnusSpawnedObject, GameObject>>();

		private List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs;

        private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

		private IDisposable disp;

		private CavrnusSpaceConnection spaceConn;

		public CavrnusObjectCreationHandler(List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs, CavrnusSpaceConnection spaceConn)
		{
			this.spaceConn = spaceConn;
			this.spawnablePrefabs = spawnablePrefabs;

			var creationHandler = spaceConn.RoomSystem.LiveJournal.GetObjectCreationReciever();

			var visibleObjectOps = creationHandler.GetMultiEntryWatcher<OpCreateObjectLive>().ActiveOps;

			//We have the delay to allow for the new objct's properties to arrive.  This way there's less weird "pop-in" of values
			disp = visibleObjectOps.BindAll(op => CavrnusStatics.Scheduler.ExecInMainThreadAfterFrames(3, () => ObjectCreated(op)), ObjectRemoved);
		}

		public void Dispose() { disp.Dispose(); }

		internal void ObjectCreated(OpInfo<OpCreateObjectLive> createOp)
		{
			//Since we waited, this should be here if they already set it
			var initialTransform = spaceConn.GetTransformPropertyValue(createOp.Op.ObjectContextPath.ToString(), "Transform");

			if (createOp.Op.ObjectType is ContentTypeWellKnownId cId) 
			{
				if (spawnablePrefabs.Any(sp => sp.UniqueId == cId.WellKnownId)) 
				{
					var prefab = spawnablePrefabs.FirstOrDefault(sp => sp.UniqueId == cId.WellKnownId)?.Object;
					
                    var ob = GameObject.Instantiate(prefab, initialTransform.Position, Quaternion.Euler(initialTransform.EulerAngles));
					createdObjects[createOp.Op.ObjectContextPath.ToString()] = ob.gameObject;
					ob.gameObject.name = $"{createOp.Op.ObjectContextPath.ToString()} ({prefab.name})";
					var spawnedObject = new CavrnusSpawnedObject(createOp.Op.ObjectContextPath.ToString(), ob, createOp.Id, spaceConn);
					ob.gameObject.AddComponent<CavrnusSpawnedObjectFlag>().Init(spawnedObject);
					CavrnusPropertyHelpers.ResetLiveHierarchyRootName(ob.gameObject, createOp.Op.ObjectContextPath.ToString());

					if(SpawnCallbacks.ContainsKey(createOp.Op.ObjectContextPath.ToString()))
					{
						SpawnCallbacks[createOp.Op.ObjectContextPath.ToString()].Invoke(spawnedObject, ob);
						SpawnCallbacks.Remove(createOp.Op.ObjectContextPath.ToString());
					}
				}
				else {
					Debug.LogWarning(
						$"Could not find spawnable prefab with ID {cId.WellKnownId} in the \"Cavrnus Spatial Connector\"");
				}
			}
			/*else if(createOp.Op.ObjectType is ContentTypeId ctId)
			{
				if(!spawnablePrefabs.Any((sp => sp.UniqueId == "HoloLoader")))
				{
					//TODO: CHECK FILE TYPE FOR COMPATABILITY!!!!!!!!!!!!!!!
					Debug.LogWarning("Cannot load file since HoloLoader is not present in Spawnable Prefabs");
					return;
				}

				var prefab = spawnablePrefabs.FirstOrDefault(sp => sp.UniqueId == "HoloLoader")?.Object;
				var ob = GameObject.Instantiate(prefab, initialTransform.LocalPosition, Quaternion.Euler(initialTransform.LocalEulerAngles));
				createdObjects[createOp.Op.NewObjectId] = ob.gameObject;
				ob.gameObject.name = $"{createOp.Op.NewObjectId} ({prefab.name})";
				ob.gameObject.AddComponent<CavrnusSpawnedObjectFlag>().Init(new CavrnusSpawnedObject(createOp.Op.NewObjectId, createOp.Id, spaceConn));
				CavrnusPropertyHelpers.ResetLiveHierarchyRootName(ob.gameObject, createOp.Op.NewObjectId);
			}*/
			else if (createOp.Op.ObjectType is ContentTypeUrl cUrl) {
				Debug.LogWarning($"ContentType URL coming soon...");
			}
			else {
				Debug.LogWarning($"ContentType {createOp.Op.ObjectType} is not currently supported by the Cavrnus SDK.");
			}
		}

		internal void ObjectRemoved(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createdObjects.ContainsKey(createOp.Op.ObjectContextPath.ToString())) {
				GameObject.Destroy(createdObjects[createOp.Op.ObjectContextPath.ToString()]);
				createdObjects.Remove(createOp.Op.ObjectContextPath.ToString());
			}
		}
	}
}