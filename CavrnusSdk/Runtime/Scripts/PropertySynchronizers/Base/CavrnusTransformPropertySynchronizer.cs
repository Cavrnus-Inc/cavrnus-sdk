using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusTransformPropertySynchronizer : CavrnusValueSync<CavrnusTransformData>
	{
		//TODO: EXPOSE DELTA

		private void Reset() { PropertyName = "Transform"; }
	}
}