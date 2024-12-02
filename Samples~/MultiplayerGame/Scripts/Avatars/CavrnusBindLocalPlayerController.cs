using CavrnusSdk.API;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusBindLocalPlayerController : MonoBehaviour
    {
        [SerializeField] private CavrnusAvatarInputReceiver inputReceiver;
        
        private Vector3 input;
        private CavrnusSpaceConnection spaceConn;
        private CavrnusLivePropertyUpdate<Vector4> updater;
        private CavrnusUser localUser;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                sc.AwaitLocalUser(lu => {
                    spaceConn = sc;
                    localUser = lu;
                    updater = null;
                    updater = spaceConn.BeginTransientVectorPropertyUpdate(
                        lu.ContainerId, CavrnusPropertyInfo.PlayerInputProperty, Vector4.zero);
                });
            });
        }
        
        private void Update()
        {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }

        private void FixedUpdate()
        {
            if (inputReceiver != null) {
                inputReceiver.HandleMovementInput(input);

                if (spaceConn != null) {
                    updater ??= spaceConn.BeginTransientVectorPropertyUpdate(localUser.ContainerId, CavrnusPropertyInfo.PlayerInputProperty, input);
                    updater?.UpdateWithNewData(input.ToFloat3().ToVec3());
                    updater = null;
                }
            }
            else
                Debug.LogWarning("InputReceiver is not assigned!");
        }
    }
}