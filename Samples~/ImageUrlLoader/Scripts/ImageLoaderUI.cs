using System;
using CavrnusSdk.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.CollaborationExamples
{
    public class ImageLoaderUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;

        private CavrnusSpaceConnection spaceConn;
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(connection => {
                spaceConn = connection;
                submitButton.onClick.AddListener(OnSubmit);
            });
        }

        private void OnSubmit()
        {
            var inputVal = inputField.text;
            inputField.text = "";
            spaceConn?.SpawnObject("ImageUrlLoader", (spawnedObject, go) => {
                spaceConn.PostStringPropertyUpdate(spawnedObject.PropertiesContainerName, "Url", inputVal);
            });
        }

        private void OnDestroy()
        {
            submitButton.onClick.RemoveListener(OnSubmit);
        }
    }
}