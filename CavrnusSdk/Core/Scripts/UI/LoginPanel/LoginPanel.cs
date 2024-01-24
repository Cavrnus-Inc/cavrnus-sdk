using System;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.UI
{
	public class LoginPanel : MonoBehaviour
	{
		[SerializeField] private TMP_InputField server;
		[SerializeField] private TMP_InputField email;
		[SerializeField] private TMP_InputField password;

		public async void Authenticate()
		{
			try { await CavrnusHelpers.Authenticate(server.text, email.text, password.text); }
			catch (Exception e) {
				Debug.Log(e.ToString());
				return;
			}
		}
	}
}