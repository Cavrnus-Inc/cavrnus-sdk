using UnityEngine;

namespace CavrnusSdk.XR.UiPositioners
{
    public class XrSphereUIPositioner : MonoBehaviour
    {
        public float SphereRadius = 1.0f;
        public float MoveSpeed = 2.0f;
        
        [SerializeField] private float maxDistanceFromCamera = 1f;
        [SerializeField] private bool debugDisplaySphere = false;
        [SerializeField] private bool debugDisplayTargetPosition = false;
        
        private Vector3 targetPosition;
        private Vector3 optimalPosition;
        
        private void Update()
        {
            var forward = XrRigHelper.Instance.EyePosition.forward;
            
            optimalPosition = XrRigHelper.Instance.EyePosition.position + forward * maxDistanceFromCamera;
            var offsetDir = transform.position - optimalPosition;
            
            if (offsetDir.magnitude > SphereRadius)
            {
                targetPosition = optimalPosition + offsetDir.normalized * SphereRadius;
                transform.position = Vector3.Lerp(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
            }
        }
        
        public void OnDrawGizmos()
        {
            if (Application.isPlaying == false) 
                return;
            
            var oldColor = Gizmos.color;
            
            if (debugDisplaySphere)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(optimalPosition, SphereRadius);
            }
            
            if (debugDisplayTargetPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(targetPosition, new Vector3(0.1f, 0.1f, 0.1f));
            }
            
            Gizmos.color = oldColor;
        }
    }
}