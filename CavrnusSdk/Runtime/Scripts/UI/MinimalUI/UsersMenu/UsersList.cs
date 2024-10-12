using System;
using System.Collections.Generic;
using CavrnusSdk.UI;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.UI
{
    public class UsersList : MonoBehaviour
    {
        public event Action<CavrnusUser> SelectedUser;
        
        [SerializeField] private UsersListEntry entryPrefab;
        [SerializeField] private Transform container;

        private readonly Dictionary<string, UsersListEntry> menuInstances = new Dictionary<string, UsersListEntry>();

        private CavrnusSpaceConnection spaceConn;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                spaceConn.BindSpaceUsers(UserAdded, UserRemoved);
            });
        }
        
        private void UserAdded(CavrnusUser user)
        {
            var go = Instantiate(entryPrefab, container);
            menuInstances[user.ContainerId] = go;
            menuInstances[user.ContainerId].Setup(user, MaximizedUserSelected);
        }

        private void MaximizedUserSelected(CavrnusUser user)
        {
            SelectedUser?.Invoke(user);

            if (CavrnusMainMenuManager.Instance != null) {
                CavrnusMainMenuManager.Instance.MaximizedUserManager.LoadUser(user);
            }
        }

        private void UserRemoved(CavrnusUser user)
        {
            if (menuInstances.ContainsKey(user.ContainerId)) {
                Destroy(menuInstances[user.ContainerId].gameObject);
                menuInstances.Remove(user.ContainerId);
            }
        }
    }
}