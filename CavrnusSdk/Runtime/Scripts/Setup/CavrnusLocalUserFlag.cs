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

                    //Transform is SEND-ONLY
					foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
					{
						sync.RecieveChanges = false;
						sync.Setup();
					}

                    //All others are RECV-ONLY
					foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
                    {
                        sync.SendMyChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
                    {
                        sync.SendMyChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
                    {
                        sync.SendMyChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
                    {
                        sync.SendMyChanges = false;
                        sync.Setup();
                    }
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
                    {
                        sync.SendMyChanges = false;
                        sync.Setup();
                    }
                });				
			});
		}
	}
}