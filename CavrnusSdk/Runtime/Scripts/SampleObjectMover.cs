using UnityEngine;

namespace CavrnusSdk
{
	public class SampleObjectMover : MonoBehaviour
	{
		private bool moving;

		void Update()
		{
			//Clickity Clackity
			//Detect if the user clicked/dragged me.  Just uses Unity stuff
			if (Input.GetMouseButtonDown(0)) {
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit2;

				if (Physics.Raycast(ray, out hit2, 100)) {
					if (hit2.collider != null && hit2.collider.transform == transform) { moving = true; }
				}
			}

			if (Input.GetMouseButtonUp(0)) { moving = false; }

			if (moving) {
				Plane obPlane = new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, -5.35f));
				var planeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				obPlane.Raycast(planeRay, out float planeHit);

				transform.position = planeRay.GetPoint(planeHit);
			}

			if (Input.GetMouseButtonDown(1)) {
				UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100)) {
					if (hit.collider != null && hit.collider.transform == transform) {
						//Create some new value.  Can come from UI or whatever you want
						var newRot = UnityEngine.Random.rotation.eulerAngles;

						var newScl = transform.localScale;
						if (newScl.x >= 1)
							newScl -= new Vector3(.2f, .2f, .2f);
						else
							newScl += new Vector3(.2f, .2f, .2f);

						transform.localEulerAngles = newRot;
						transform.localScale = newScl;
					}
				}
			}
		}
	}
}