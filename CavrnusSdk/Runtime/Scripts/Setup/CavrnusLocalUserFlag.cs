using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using CavrnusCore;

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

                    CavrnusPropertyHelpers.ResetLiveHierarchyRootName(gameObject, $"{localUser.ContainerId}");

                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
                    {
                        sync.RecieveChanges = false;
                        sync.Setup();
                    }
                });				
			});
		}
	}
}