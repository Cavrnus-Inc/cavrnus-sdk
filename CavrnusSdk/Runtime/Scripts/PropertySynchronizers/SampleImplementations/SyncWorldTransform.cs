using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncWorldTransform : CavrnusValueSyncTransform
    {
        public override CavrnusTransformData GetValue()
        {
            return new CavrnusTransformData(transform.position, transform.eulerAngles, transform.lossyScale);
        }

        public override void SetValue(CavrnusTransformData value)
        {
            transform.position = value.Position;
            transform.eulerAngles = value.EulerAngles;
            transform.localScale = value.Scale;
        }
    }
}