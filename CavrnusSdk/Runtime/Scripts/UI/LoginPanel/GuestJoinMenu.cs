using TMPro;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.Setup;

namespace CavrnusSdk.UI
{
    public class GuestJoinMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;

        public void Authenticate()
        {
            CavrnusFunctionLibrary.AuthenticateAsGuest(CavrnusSpatialConnector.Instance.MyServer, nameField.text, auth => { }, err => Debug.LogError(err));
        }
    }
}