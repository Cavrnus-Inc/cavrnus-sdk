using UnityEngine;

namespace CavrnusSdk
{
	public class SampleDeleteSpawnedObject : MonoBehaviour
	{
		CavrnusSpaceConnection spaceConn;
		void Start() { CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => spaceConn = sc); }

		// Update is called once per frame
		void Update()
		{
			if (spaceConn == null) return;

			//Clickity Clackity
			//Detect if the user clicked me.  Just uses Unity stuff
			if (Input.GetMouseButtonDown(1)) {
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100)) {
					if (hit.collider != null && hit.collider.transform == transform) { DeleteObject(); }
				}
			}
		}

		private void DeleteObject()
		{
			CavrnusHelpers.DeleteSpawnedObject(spaceConn, GetComponent<CavrnusSpawnedObject>());
		}
	}
}