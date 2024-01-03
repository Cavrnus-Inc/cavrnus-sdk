using System;
using UnityEngine;

namespace CavrnusSdk.Permissions
{
    public class PermissionsControlledVisibility : MonoBehaviour
    {
        [SerializeField] private string permissionAction;
        
        private IDisposable disposable;
        private CavrnusPropertiesContainer ctx;

        private void Start()
        {
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => {
                disposable = RoleAndPermissionHelpers.EvaluateSpacePolicy(permissionAction, sc, b => {
                    gameObject.SetActive(b);
                });
            });
        }

        private void OnDestroy() => disposable?.Dispose();
    }
}