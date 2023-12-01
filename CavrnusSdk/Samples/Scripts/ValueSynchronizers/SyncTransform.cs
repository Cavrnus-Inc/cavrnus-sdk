namespace CavrnusSdk
{
	public class SyncTransform : CavrnusTransformPropertySynchronizer
	{
		public override CavrnusPropertyHelpers.TransformData GetValue()
		{
			return new CavrnusPropertyHelpers.TransformData(transform.localPosition, transform.localEulerAngles, transform.localScale);
		}

		public override void SetValue(CavrnusPropertyHelpers.TransformData value)
		{
			transform.localPosition = value.LocalPosition;
			transform.localEulerAngles = value.LocalEulerAngles;
			transform.localScale = value.LocalScale;
		}
	}
}

