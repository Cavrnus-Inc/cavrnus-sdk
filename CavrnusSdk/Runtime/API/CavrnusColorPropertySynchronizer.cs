using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusColorPropertySynchronizer : CavrnusValueSync<Color>
	{
		private void Reset() { PropertyName = "Color"; }
	}
}