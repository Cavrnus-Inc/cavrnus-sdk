using Collab.Base.Collections;
using Collab.LiveRoomSystem;
using Collab.Proxy.LiveJournal;
using Collab.Proxy.Comm.LocalTypes;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CavrnusSdk
{
	public class CavrnusObjectCreationHandler : IDisposable
	{
		private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

		private IDisposable disp;

		public CavrnusObjectCreationHandler(RoomSystem rs)
		{
			var creationHandler = rs.LiveJournal.GetObjectCreationReciever();

			var visibleObjectOps = creationHandler.GetMultiEntryWatcher<OpCreateObjectLive>().VisibleOps;

			disp = visibleObjectOps.BindAll(ObjectCreated, ObjectRemoved);
		}

		public void Dispose() { disp.Dispose(); }

		public void ObjectCreated(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createOp.Op.ObjectType is ContentTypeWellKnownId cId) {
				var prefabToSpawn =
					CavrnusCore.Instance.SpawnablePrefabs.SpawnablePrefabs.FirstOrDefault(
						sp => sp.UniqueIdentifier.Equals(cId.WellKnownId));
				if (prefabToSpawn != null) {
					var ob = GameObject.Instantiate(prefabToSpawn);
					createdObjects[createOp.Op.NewObjectId] = ob.gameObject;
					ob.gameObject.AddComponent<CavrnusSpawnedObject>().Init(createOp.Op.NewObjectId, createOp.Id);
				}
				else {
					Debug.LogWarning(
						$"Could not find spawnable prefab with ID {cId.WellKnownId} in the \"Assets/CavrnusSDK/Cavrnus Spawnable Prefabs Lookup\"");
				}
			}
			else if (createOp.Op.ObjectType is ContentTypeUrl cUrl) {
				Debug.LogWarning($"ContentType URL coming soon...");
			}
			else {
				Debug.LogWarning($"ContentType {createOp.Op.ObjectType} is not currently supported by the Cavrnus SDK.");
			}
		}

		public void ObjectRemoved(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createdObjects.ContainsKey(createOp.Op.NewObjectId)) {
				GameObject.Destroy(createdObjects[createOp.Op.NewObjectId]);
				createdObjects.Remove(createOp.Op.NewObjectId);
			}
		}
	}
}