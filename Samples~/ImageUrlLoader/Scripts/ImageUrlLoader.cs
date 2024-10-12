using System.Collections;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace CavrnusSdk.CollaborationExamples
{
    public class ImageUrlLoader : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Texture2D> onImageLoaded;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConn.BindStringPropertyValue(GetComponent<CavrnusPropertiesContainer>().UniqueContainerName,
                                                  "Url", FetchImage);
            });
        }

        private void FetchImage(string url)
        {
            if (destroyed) return;

            if (url == null) return;

            StartCoroutine(LoadImageFromURL(url));
        }

        private IEnumerator LoadImageFromURL(string url)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError("Error loading image: " + request.error);
            else {
                var texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

                onImageLoaded?.Invoke(texture);
            }
        }

        private bool destroyed = false;
        private void OnDestroy() { destroyed = true; }
    }
}