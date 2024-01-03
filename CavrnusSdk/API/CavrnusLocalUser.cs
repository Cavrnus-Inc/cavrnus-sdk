using CavrnusSdk;
using Collab.Base.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavrnusLocalUser : MonoBehaviour
{
    void Start()
    {
		CavrnusSpaceJoinEvent.OnAnySpaceConnection(spaceConn =>
		{
			var user = spaceConn.UsersList.LocalUser.Bind(lu =>
			{
				gameObject.AddComponent<CavrnusUserFlag>().User = lu;

				foreach (var propContainer in gameObject.GetComponentsInChildren<CavrnusPropertiesContainer>())
					propContainer.PrefixContainerName($"{lu.ContainerId}");

				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
					sync.RecieveChanges = false;
				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
					sync.RecieveChanges = false;
				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
					sync.RecieveChanges = false;
				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
					sync.RecieveChanges = false;
				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusPropertyHelpers.TransformData>>())
					sync.RecieveChanges = false;
				foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
					sync.RecieveChanges = false;
			});
		});
	}
}
