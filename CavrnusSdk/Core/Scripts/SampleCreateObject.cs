using System.Linq;
using Collab.Proxy.Comm.LocalTypes;
using UnityEngine;

namespace CavrnusSdk
{
	public class SampleCreateObject : MonoBehaviour
	{
		public string UniqueObjectIdToSpawn;

		CavrnusSpaceConnection spaceConn;
		void Start() { CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => spaceConn = sc); }

		// Update is called once per frame
		void Update()
		{
			if (spaceConn == null) return;

			//Clickity Clackity
			//Detect if the user clicked me.  Just uses Unity stuff
			if (Input.GetMouseButtonDown(0)) {
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100)) {
					if (hit.collider != null && hit.collider.transform == transform) { CreateObject(); }
				}
			}
		}

		private void CreateObject()
		{
			var pos = transform.position +
			          new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));

			PostSpawnObjectWithUniqueId(spaceConn, UniqueObjectIdToSpawn,
			                                           new CavrnusPropertyHelpers.TransformData(
				                                           pos, transform.localEulerAngles, Vector3.one));
		}
		
		// Temporarily moved out of CavrnusHelpers to avoid missing dependency when using package samples.
		private string PostSpawnObjectWithUniqueId(CavrnusSpaceConnection spaceConn, string uniqueId,
		                                           CavrnusPropertyHelpers.TransformData pos = null)
		{
			var prefabToUse =
				CavrnusCore.Instance.SpawnablePrefabs.SpawnablePrefabs.FirstOrDefault(
					sp => sp.UniqueIdentifier == uniqueId);

			if (prefabToUse == null) {
				Debug.LogError(
					$"Attempting to spawn prefab with unique ID {uniqueId}, but it does not exist in \"Assets/CavrnusSdk/Cavrnus Spawnable Prefabs Lookup\".");
				return null;
			}

			var newId = spaceConn.RoomSystem.Comm.CreateNewUniqueObjectId();
			var creatorId = spaceConn.RoomSystem.Comm.LocalCommUser.Value.ConnectionId;
			var contentType = new ContentTypeWellKnownId(uniqueId);

			var createOp = new OpCreateObjectLive(null, newId, creatorId, contentType);

			spaceConn.RoomSystem.Comm.SendJournalEntry(createOp.ToOp(), null);

			if (pos != null) {
				if (prefabToUse.GetComponent<SyncTransform>() == null) {
					Debug.LogError(
						$"Attempting to set the Transform of spawned prefab with unique Id {uniqueId}, but it has no SyncTransform component.");
					return newId;
				}

				var containerPath = prefabToUse.GetComponent<CavrnusPropertiesContainer>().UniqueContainerPath.ToList();
				containerPath.Insert(0, newId);

				CavrnusPropertyHelpers.UpdateTransformProperty(spaceConn, containerPath.ToArray(),
				                                               prefabToUse.GetComponent<SyncTransform>()
				                                                          .PropertyName, pos.LocalPosition,
				                                               pos.LocalEulerAngles, pos.LocalScale);
			}

			return newId;
		}
	}
}