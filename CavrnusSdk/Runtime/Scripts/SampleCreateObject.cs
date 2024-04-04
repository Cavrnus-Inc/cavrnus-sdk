using System.Linq;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk
{
	public class SampleCreateObject : MonoBehaviour
	{
		public string UniqueObjectIdToSpawn;

		CavrnusSpaceConnection spaceConn;
		void Start() { CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => spaceConn = sc); }

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
			var randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
			var pos = transform.position + randomOffset;

			PostSpawnObjectWithUniqueId(spaceConn, UniqueObjectIdToSpawn, new CavrnusTransformData(pos, transform.localEulerAngles, Vector3.one));
		}
		
		// Temporarily moved out of CavrnusHelpers to avoid missing dependency when using package samples.
		private string PostSpawnObjectWithUniqueId(CavrnusSpaceConnection spaceConn, string uniqueId, CavrnusTransformData pos)
		{
			string newContainerName = spaceConn.SpawnObject(uniqueId);
			
			spaceConn.PostTransformPropertyUpdate(newContainerName, "Transform", pos);

			return newContainerName;
		}
	}
}