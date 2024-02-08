using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk
{
	public class SampleUnityCameraControls : MonoBehaviour
	{
		private const float moveSpeed = .1f;
		private const float rotationSpeed = 1f;

		private CavrnusSpaceConnection spaceConn;

		void Start() { CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection); }

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn) { this.spaceConn = spaceConn; }

		// Update is called once per frame
		void Update()
		{
			//We don't want us typing in a username/pwrd to trigger movement
			if (spaceConn == null) return;

			if (Input.GetKey(KeyCode.W)) transform.position += transform.forward * moveSpeed;
			if (Input.GetKey(KeyCode.S)) transform.position += -transform.forward * moveSpeed;
			if (Input.GetKey(KeyCode.D)) transform.position += transform.right * moveSpeed;
			if (Input.GetKey(KeyCode.A)) transform.position += -transform.right * moveSpeed;
			if (Input.GetKey(KeyCode.LeftControl)) transform.position += -transform.up * moveSpeed;
			if (Input.GetKey(KeyCode.Space)) transform.position += transform.up * moveSpeed;

			if (Input.GetKey(KeyCode.Q)) transform.localEulerAngles += new Vector3(0, -rotationSpeed, 0);
			if (Input.GetKey(KeyCode.E)) transform.localEulerAngles += new Vector3(0, rotationSpeed, 0);
		}
	}
}