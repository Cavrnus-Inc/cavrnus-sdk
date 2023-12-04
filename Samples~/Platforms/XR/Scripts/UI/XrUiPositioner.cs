using System;
using UnityEngine;

namespace CavrnusSdk.XR.UiPositioners
{
    public class XrUiPositioner : MonoBehaviour
    {
        [SerializeField] private float distanceFromEyes = 0.8f;
        [SerializeField] private float heightOffsetFromEyes = 0.2f;
        [SerializeField] private float xTilt = 0f;

        private void Awake()
        {
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(csc => RealignToEyeDirectionAndHeight());
        }

        private void OnEnable()
        {
            RealignToEyeDirectionAndHeight();
        }

        public void RealignToEyeDirectionAndHeight()
        {
            if (XrRigHelper.Instance.XrRig == null)
                Debug.Log("XR Rig is null! " + gameObject);

            var eyeTransform = XrRigHelper.Instance.EyePosition;
            
            var targetRotation = eyeTransform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(xTilt, targetRotation.y, 0);
            
            var adjustedForward = eyeTransform.position + eyeTransform.forward * distanceFromEyes;
            var adjustedHeight = new Vector3(adjustedForward.x, eyeTransform.position.y - heightOffsetFromEyes, adjustedForward.z);
            
            transform.position = adjustedHeight;
        }
    }
}