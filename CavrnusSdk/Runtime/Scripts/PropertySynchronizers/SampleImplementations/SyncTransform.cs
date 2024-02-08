using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncTransform : CavrnusTransformPropertySynchronizer
	{
		public override CavrnusTransformData GetValue()
		{
			return new CavrnusTransformData(transform.localPosition, transform.localEulerAngles, transform.localScale);
		}

		public override void SetValue(CavrnusTransformData value)
		{
			transform.localPosition = value.LocalPosition;
			transform.localEulerAngles = value.LocalEulerAngles;
			transform.localScale = value.LocalScale;
		}
	}
}