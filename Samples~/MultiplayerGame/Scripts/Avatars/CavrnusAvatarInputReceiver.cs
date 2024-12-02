using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusAvatarInputReceiver : MonoBehaviour
    {
        public Vector3 Input{ get; private set; }
        
        [SerializeField] private Transform model;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnSpeed = 360f;

        public void HandleMovementInput(Vector3 input)
        {
            Input = input;

            var movement = Input.ToIso() * Input.normalized.magnitude * speed * Time.deltaTime;
            transform.position += movement;

            HandleRotation();
        }

        private void HandleRotation()
        {
            if (Input == Vector3.zero) return;

            var targetRotation = Quaternion.LookRotation(Input.ToIso(), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}