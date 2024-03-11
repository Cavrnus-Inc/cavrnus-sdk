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

					foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
					{
						sync.Setup();
                        //Force init user props
                        if(sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
                            sync.ForceBeginTransientUpdate();
					}
					foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
                    {
                        sync.Setup();
						//Force init user props
						if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
							sync.ForceBeginTransientUpdate();
					}
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
                    {
                        sync.Setup();
						//Force init user props
						if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
							sync.ForceBeginTransientUpdate();
					}
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
                    {
                        sync.Setup();
						//Force init user props
						if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
							sync.ForceBeginTransientUpdate();
					}
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
                    {
                        sync.Setup();
						//Force init user props
						if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
							sync.ForceBeginTransientUpdate();
					}
                    foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
                    {
                        sync.Setup();
						//Force init user props
						if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(localUser.ContainerId))
							sync.ForceBeginTransientUpdate();
					}
                });				
			});
		}
	}
}