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

			CavrnusHelpers.PostSpawnObjectWithUniqueId(spaceConn, UniqueObjectIdToSpawn,
			                                           new CavrnusPropertyHelpers.TransformData(
				                                           pos, transform.localEulerAngles, Vector3.one));
		}
	}
}