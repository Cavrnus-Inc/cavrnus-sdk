using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;

namespace CavrnusSdk.Setup
{
	public class CavrnusAvatarManager : MonoBehaviour
	{
		[Header("Avatar that will represent other remote users in the scene")]
	    public GameObject RemoteAvatarPrefab;

	    private Dictionary<string, GameObject> avatarInstances = new Dictionary<string, GameObject>();

		// Start is called before the first frame update
		void Start()
	    {
		    if (RemoteAvatarPrefab == null)
		    {
			    Debug.LogError("No Avatar Prefab has been assigned.  Shutting down CoPresence display system.");
				return;
		    }

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection);

		}

		private CavrnusSpaceConnection cavrnusSpaceConnection = null;
		private void OnSpaceConnection(CavrnusSpaceConnection obj)
		{
			cavrnusSpaceConnection = obj;

			CavrnusFunctionLibrary.BindSpaceUsers(cavrnusSpaceConnection, UserAdded, UserRemoved);
		}

		//Instantiate avatars when we get a new user
		private void UserAdded(CavrnusUser user)
		{
			//This list contains the player.  But we don't wanna show their avatar via this system.
			if (user.IsLocalUser)
				return;

			var avatar = Instantiate(RemoteAvatarPrefab, transform);

			avatar.AddComponent<CavrnusUserFlag>().User = user;

			foreach (var propContainer in avatar.GetComponentsInChildren<CavrnusPropertiesContainer>())
				propContainer.PrefixContainerName($"{user.ContainerId}");

			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
				sync.SendMyChanges = false;

			avatarInstances[user.ContainerId] = avatar;
		}

		//Destroy them when we lose that user
		private void UserRemoved(CavrnusUser user)
		{
			if (avatarInstances.ContainsKey(user.ContainerId))
			{
				Destroy(avatarInstances[user.ContainerId].gameObject);
				avatarInstances.Remove(user.ContainerId);
			}
		}
	}
}