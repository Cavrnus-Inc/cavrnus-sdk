using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusStringPropertySynchronizer : CavrnusValueSync<string>
	{
		private void Reset() { PropertyName = "Text"; }
	}
}