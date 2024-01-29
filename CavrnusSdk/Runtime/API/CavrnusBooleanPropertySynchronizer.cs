using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusBooleanPropertySynchronizer : CavrnusValueSync<bool>
	{
		private void Reset() { PropertyName = "Boolean"; }
	}
}