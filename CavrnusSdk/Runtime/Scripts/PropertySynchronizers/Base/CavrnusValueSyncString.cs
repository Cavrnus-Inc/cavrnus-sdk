using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncString : CavrnusValueSync<string>
	{
		private void Reset() { PropertyName = "Text"; }
	}
}