using CavrnusSdk.API;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CavrnusCore.Library
{
    public class HoloObjectCreation : MonoBehaviour
    {
        [SerializeField] private HoloContentLibrary library;
        
        private CavrnusSpaceConnection spaceConn;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                library.OnSelect += CreateObject;
            });
        }

        private void CreateObject(CavrnusRemoteContent obj)
        {
            var randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
            var pos = Vector3.zero + randomOffset;
            PostSpawnObjectWithUniqueId(obj,"HoloLoader", new CavrnusTransformData(pos, transform.localEulerAngles, Vector3.one));
        }

        private void PostSpawnObjectWithUniqueId(CavrnusRemoteContent contentToUse, string uniqueId, CavrnusTransformData pos = null)
        {
            string newContainerName = spaceConn.SpawnObject(uniqueId);

            spaceConn.PostStringPropertyUpdate(newContainerName, "ContentId", contentToUse.Id);

            if (pos != null)
                spaceConn.PostTransformPropertyUpdate(newContainerName, "Transform", pos);
        }
    }
}