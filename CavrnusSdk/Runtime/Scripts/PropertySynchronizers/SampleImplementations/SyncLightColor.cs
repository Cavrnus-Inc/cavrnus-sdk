using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncLightColor : CavrnusValueSyncColor
    {
        public override Color GetValue()
        {
            return GetComponent<Light>().color;
            //throw new System.NotImplementedException();
        }

        public override void SetValue(Color value)
        {
            GetComponent<Light>().color = value;
            //throw new System.NotImplementedException();
        }
    }
}