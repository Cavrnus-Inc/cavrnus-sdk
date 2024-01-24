using Collab.Base.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.UI
{
	public class UsersPanel : MonoBehaviour
	{
		[SerializeField] private UsersPanelEntry entryPrefab;
		[SerializeField] private Transform contentParent;

		private Dictionary<string, UsersPanelEntry> menuInstances = new Dictionary<string, UsersPanelEntry>();

		void Start()
		{
			CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);
		}

		private CavrnusSpaceConnection cavrnusSpaceConnection;

		private void OnSpaceConnection(CavrnusSpaceConnection obj)
		{
			cavrnusSpaceConnection = obj;

			cavrnusSpaceConnection.UsersList.Users.BindAll(UserAdded, UserRemoved);
		}

		private void UserAdded(CavrnusUser user)
		{
			menuInstances[user.ContainerId] = GameObject.Instantiate(entryPrefab, contentParent);
			menuInstances[user.ContainerId].Setup(user);
		}

		private void UserRemoved(CavrnusUser user)
		{
			if (menuInstances.ContainsKey(user.ContainerId)) {
				GameObject.Destroy(menuInstances[user.ContainerId].gameObject);
				menuInstances.Remove(user.ContainerId);
			}
		}
	}
}