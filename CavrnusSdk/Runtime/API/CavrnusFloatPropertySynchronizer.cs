using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusFloatPropertySynchronizer : CavrnusValueSync<float>
	{
		private void Reset() { PropertyName = "Float"; }
	}
}