using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusColorPropertySynchronizer : CavrnusValueSync<Color>
	{
		private void Reset() { PropertyName = "Color"; }
	}
}