using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusStringPropertySynchronizer : CavrnusValueSync<string>
	{
		private void Reset() { PropertyName = "Text"; }
	}
}