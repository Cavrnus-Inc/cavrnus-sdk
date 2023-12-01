using System.Collections.Generic;
using CavrnusSdk.Avatars;
using UnityEngine;
using Collab.Base.Collections;
using Collab.Proxy.Prop.JournalInterop;

namespace CavrnusSdk
{
	public class CavrnusAvatarManager : MonoBehaviour
	{
	    public GameObject AvatarPrefab;

	    private Dictionary<string, GameObject> avatarInstances = new Dictionary<string, GameObject>();

		// Start is called before the first frame update
		void Start()
	    {
		    if (AvatarPrefab == null)
		    {
			    Debug.LogError("No Avatar Prefab has been assigned.  Shutting down CoPresence display system.");
				return;
		    }	

			CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);

		}

		private CavrnusSpaceConnection cavrnusSpaceConnection = null;
		private void OnSpaceConnection(CavrnusSpaceConnection obj)
		{
			cavrnusSpaceConnection = obj;

			cavrnusSpaceConnection.UsersList.Users.BindAll(UserAdded, UserRemoved);
		}

		//Instantiate avatars when we get a new user
		private void UserAdded(CavrnusUser user)
		{
			//This list contains the player.  But we don't wanna show their avatar via this system.
			if (user.IsLocalUser)
				return;

			avatarInstances[user.ContainerId] = Instantiate(AvatarPrefab);

			avatarInstances[user.ContainerId].GetComponent<SyncTransform>().GetComponent<CavrnusPropertiesContainer>().SetContainerName($"{user.ContainerId}");
			avatarInstances[user.ContainerId].GetComponent<SyncTransform>().PropertyName = UserPropertyDefs.User_CopresenceLocation;

			avatarInstances[user.ContainerId].GetComponentInChildren<SyncTmpText>().GetComponent<CavrnusPropertiesContainer>().SetContainerName($"{user.ContainerId}");
			avatarInstances[user.ContainerId].GetComponentInChildren<SyncTmpText>().PropertyName = UserPropertyDefs.Users_Name;
			
			avatarInstances[user.ContainerId].GetComponentInChildren<AvatarTag>().Setup(user);
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