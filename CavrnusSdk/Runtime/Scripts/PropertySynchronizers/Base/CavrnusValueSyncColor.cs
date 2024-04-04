using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncColor : CavrnusValueSync<Color>
	{
		private void Reset() { PropertyName = "Color"; }
	}
}