using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusAvatarAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CavrnusAvatarInputReceiver inputReceiver;
        
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int EmoteReset = Animator.StringToHash("EmoteReset");

        private void Update()
        {
            if (inputReceiver.Input != Vector3.zero && didEmote)
            {
                animator.SetTrigger(EmoteReset);
                didEmote = false;
            }

            var speed = inputReceiver.Input.magnitude;
            animator.SetFloat(Speed, speed, 0.1f, Time.deltaTime);
        }

        private bool didEmote = false;
        public void TriggerEmote(string emoteName)
        {
            animator.SetTrigger(emoteName);
            didEmote = true;
        }
    }
}