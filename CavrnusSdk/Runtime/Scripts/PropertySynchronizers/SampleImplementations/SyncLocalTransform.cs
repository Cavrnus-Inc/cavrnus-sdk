using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncLocalTransform : CavrnusValueSyncTransform
	{
		public override CavrnusTransformData GetValue()
		{
			return new CavrnusTransformData(transform.localPosition, transform.localEulerAngles, transform.localScale);
		}

		public override void SetValue(CavrnusTransformData value)
		{
			transform.localPosition = value.Position;
			transform.localEulerAngles = value.EulerAngles;
			transform.localScale = value.Scale;
		}
	}
}