using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusVectorPropertySynchronizer : CavrnusValueSync<Vector4>
	{
		private void Reset() { PropertyName = "Vector"; }
	}
}