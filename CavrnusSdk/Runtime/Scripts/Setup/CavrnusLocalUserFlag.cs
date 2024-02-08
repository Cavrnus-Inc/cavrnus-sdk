using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;

namespace CavrnusSdk.Setup
{
	public class CavrnusLocalUserFlag : MonoBehaviour
	{
	    private void Start()
	    {
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
			{
				spaceConn.AwaitLocalUser(localUser =>
				{
                    gameObject.AddComponent<CavrnusUserFlag>().User = localUser;

                    foreach (var propContainer in gameObject.GetComponentsInChildren<CavrnusPropertiesContainer>())
                        propContainer.PrefixContainerName($"{localUser.ContainerId}");

                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
                        sync.RecieveChanges = false;
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
                        sync.RecieveChanges = false;
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
                        sync.RecieveChanges = false;
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
                        sync.RecieveChanges = false;
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
                        sync.RecieveChanges = false;
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
                        sync.RecieveChanges = false;
                });				
			});
		}
	}
}