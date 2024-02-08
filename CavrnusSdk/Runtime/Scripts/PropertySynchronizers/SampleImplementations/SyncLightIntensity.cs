using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncLightIntensity : CavrnusFloatPropertySynchronizer
    {
        public override float GetValue()
        {
            return GetComponent<Light>().intensity;
        }

        public override void SetValue(float value)
        {
            GetComponent<Light>().intensity = value;
        }
    }
}