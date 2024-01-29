using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusVectorPropertySynchronizer : CavrnusValueSync<Vector4>
	{
		private void Reset() { PropertyName = "Vector"; }
	}
}