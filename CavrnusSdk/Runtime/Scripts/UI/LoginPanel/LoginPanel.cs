using TMPro;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.Setup;

namespace CavrnusSdk.UI
{
	public class LoginPanel : MonoBehaviour
	{
		[SerializeField] private TMP_InputField email;
		[SerializeField] private TMP_InputField password;

		public void Authenticate()
		{
			CavrnusFunctionLibrary.AuthenticateWithPassword(CavrnusSpatialConnector.Instance.MyServer, email.text, password.text, auth => { }, err => Debug.LogError(err));
		}
	}
}