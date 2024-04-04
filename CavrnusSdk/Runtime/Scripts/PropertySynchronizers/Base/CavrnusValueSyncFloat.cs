using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncFloat : CavrnusValueSync<float>
	{
		private void Reset() { PropertyName = "Float"; }
	}
}