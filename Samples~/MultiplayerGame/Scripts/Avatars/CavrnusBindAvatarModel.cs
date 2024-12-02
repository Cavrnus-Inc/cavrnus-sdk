using System;
using System.Collections;
using System.Linq;
using CavrnusCore;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusBindAvatarModel : MonoBehaviour
    {
        [SerializeField] private bool isLocal;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform modelContainer;
        [SerializeField] private CavrnusAvatarTypes avatars;

        private GameObject currentAvatar;
        private IDisposable binding;

        private void Awake()
        {
            modelContainer.DestroyAllChildren();
        }

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                gameObject.GetCavrnusUserFlagInParent(cf => {
                    if (isLocal) {
                        sc.AwaitLocalUser(lu => {
                            binding = lu.BindToUserMetadata(CavrnusPropertyInfo.PlayerAvatarProperty, OnPropertyUpdated);
                        });
                    }
                    else 
                        binding = cf.User.BindToUserMetadata(CavrnusPropertyInfo.PlayerAvatarProperty, OnPropertyUpdated);
                });
            });
        }

        private void OnPropertyUpdated(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar)) {
                return;
            }
            
            if (currentAvatar != null)
                Destroy(currentAvatar);
            
            var found = avatars.AvatarTypes.FirstOrDefault(
                at => string.Equals(at.Name, avatar, StringComparison.InvariantCultureIgnoreCase));

            if (found != null) {
                var newAvatar = Instantiate(found.Prefab, modelContainer);
                animator.avatar = found.Avatar;

                currentAvatar = newAvatar;
                currentAvatar.SetActive(false);
                CavrnusStatics.Scheduler.ExecCoRoutine(ResetAnimatorWithDelay());
            }
        }

        private IEnumerator ResetAnimatorWithDelay()
        {
            animator.enabled = false;
            yield return null;
            animator.enabled = true;
            animator.Rebind();
            
            if (currentAvatar)
                currentAvatar.SetActive(true);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}