using TMPro;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.Setup;

namespace CavrnusSdk.UI
{
    public class ServerMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField serverField;

        public void ConfirmDomain()
        {
            CavrnusSpatialConnector.Instance.MyServer = serverField.text;
            CavrnusSpatialConnector.Instance.Startup();

		}
    }
}