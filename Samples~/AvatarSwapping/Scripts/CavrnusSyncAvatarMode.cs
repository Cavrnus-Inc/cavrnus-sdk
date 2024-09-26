using CavrnusSdk.PropertySynchronizers;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusSyncAvatarMode : CavrnusValueSyncString
    {
        public GameObject SphereMode;
        public GameObject CubeModeAvatar;
        public Transform TargetContainer;
        
        public override string GetValue()
        {
            if (TargetContainer.GetChild(0).gameObject.name.ToLowerInvariant().Contains("sphere"))
                return "sphere";
            else
                return "cube";
        }

        public override void SetValue(string value)
        {
            if (value == "sphere")
            {
                //Destroy the old avatar
               Destroy(TargetContainer.GetChild(0).gameObject);

                //Spawn the new avatar
               Instantiate(SphereMode, TargetContainer);
            }
            else
            {
                //Destroy the old avatar
               Destroy(TargetContainer.GetChild(0).gameObject);

                //Spawn the new avatar
               Instantiate(CubeModeAvatar, TargetContainer);
            }
        }
    }
}