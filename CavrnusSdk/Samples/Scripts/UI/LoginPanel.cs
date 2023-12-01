using System;
using TMPro;
using UnityEngine;

namespace CavrnusSdk
{
	public class LoginPanel : MonoBehaviour
	{
		[SerializeField] private TMP_InputField server;
		[SerializeField] private TMP_InputField email;
		[SerializeField] private TMP_InputField password;

		[SerializeField] private GameObject spacePickerPrefab;

		void Start() { CavrnusHelpers.Setup(); }

		public async void JoinCavrnusSpace()
		{
			try { await CavrnusHelpers.Authenticate(server.text, email.text, password.text); }
			catch (Exception e) {
				Debug.Log(e.ToString());
				return;
			}

			var foundSpaces = await CavrnusHelpers.GetAllAvailableSpaces();

			GameObject.Instantiate(spacePickerPrefab, transform.parent).GetComponent<SpacePicker>().Setup(foundSpaces);

			GameObject.Destroy(gameObject);
		}
	}
}