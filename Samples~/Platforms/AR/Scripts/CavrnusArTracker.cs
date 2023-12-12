using UnityEngine;

namespace CavrnusSdk.AR
{
    public class CavrnusArTracker : MonoBehaviour
    {
        private CavrnusArWorld arWorld;

        public void TrackArTracker(Vector3 worldPosition, Quaternion rotation)
        {
            arWorld = GetComponentInParent<CavrnusArWorld>();
            
            if (arWorld == null)
            {
                Debug.LogError($"{nameof(arWorld)} is null! Ensure it exists on parent GameObject.");
                return;
            }
            
            arWorld.transform.SetPositionAndRotation(worldPosition, rotation);
        }
    }
}