using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using CavrnusCore;
using Collab.Base.Collections;
using UnityBase;

namespace CavrnusSdk.Setup
{
	public class CavrnusLocalUserFlag : MonoBehaviour
	{
	    private void Start()
	    {
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
			{
				if (HelperFunctions.NullOrDestroyed(this))	return;

				spaceConn.AwaitLocalUser(localUser =>
				{
					if (HelperFunctions.NullOrDestroyed(this)) return;

					gameObject.AddComponent<CavrnusUserFlag>().User = localUser;

					localUser.ContainerIdSetting.Bind(lucid =>
					{
						CavrnusPropertyHelpers.ResetLiveHierarchyRootName(gameObject, $"{lucid}");

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}
					});
				});				
			});
		}
	}
}