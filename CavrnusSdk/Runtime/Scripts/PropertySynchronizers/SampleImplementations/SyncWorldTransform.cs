using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncWorldTransform : CavrnusTransformPropertySynchronizer
    {
        public override CavrnusTransformData GetValue()
        {
            return new CavrnusTransformData(transform.position, transform.eulerAngles, transform.lossyScale);
        }

        public override void SetValue(CavrnusTransformData value)
        {
            transform.position = value.LocalPosition;
            transform.eulerAngles = value.LocalEulerAngles;
            transform.localScale = value.LocalScale;
        }
    }
}