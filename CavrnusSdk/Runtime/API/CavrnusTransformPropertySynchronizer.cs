using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusTransformPropertySynchronizer : CavrnusValueSync<CavrnusPropertyHelpers.TransformData>
	{
		private void Reset() { PropertyName = "Transform"; }
	}
}