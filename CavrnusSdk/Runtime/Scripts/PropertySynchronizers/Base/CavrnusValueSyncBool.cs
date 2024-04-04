using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncBool : CavrnusValueSync<bool>
	{
		private void Reset() { PropertyName = "Boolean"; }
	}
}