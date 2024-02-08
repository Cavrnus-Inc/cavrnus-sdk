using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusFloatPropertySynchronizer : CavrnusValueSync<float>
	{
		private void Reset() { PropertyName = "Float"; }
	}
}