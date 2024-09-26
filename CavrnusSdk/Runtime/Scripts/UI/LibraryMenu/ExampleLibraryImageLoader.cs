using System.Collections.Generic;
using System.IO;
using CavrnusSdk.API;
using CavrnusSdk.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CavrnusCore.Library
{
    public class ExampleLibraryImageLoader : MonoBehaviour
    {
        [SerializeField] private LibraryMenu libraryMenu;
        
        private CavrnusSpaceConnection spaceConn;

        private readonly List<string> extensions = new List<string> {".jpg", ".jpeg", ".png"};

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                libraryMenu.OnSelect += CreateObject;
            });
        }

        private void CreateObject(CavrnusRemoteContent obj)
        {
            var ext = Path.GetExtension(obj.FileName).ToLowerInvariant();
            if (extensions.Contains(ext)) {
                var randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                var pos = Vector3.zero + randomOffset;
                var rot = Quaternion.Euler(-90f, 0f, 0f); 
                PostSpawnObjectWithUniqueId(obj,"ImageLoader", new CavrnusTransformData(pos, rot.eulerAngles, Vector3.one));
            }
        }

        private void PostSpawnObjectWithUniqueId(CavrnusRemoteContent contentToUse, string uniqueId, CavrnusTransformData pos = null)
        {
            string newContainerName = spaceConn.SpawnObject(uniqueId);

            spaceConn.PostStringPropertyUpdate(newContainerName, "ContentId", contentToUse.Id);

            if (pos != null)
                spaceConn.PostTransformPropertyUpdate(newContainerName, "Transform", pos);
        }

        private void OnDestroy()
        {
            libraryMenu.OnSelect -= CreateObject;
        }
    }
}