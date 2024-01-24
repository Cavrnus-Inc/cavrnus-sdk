using UnityEngine;

namespace CavrnusSdk
{
    public class SyncLightColor : CavrnusColorPropertySynchronizer
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