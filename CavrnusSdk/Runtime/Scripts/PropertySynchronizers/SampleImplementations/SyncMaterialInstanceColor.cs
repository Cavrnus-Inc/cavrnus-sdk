using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncMaterialInstanceColor : CavrnusValueSyncColor
	{
		public override Color GetValue()
		{
			return GetComponent<MeshRenderer>().material.color;
		}

		public override void SetValue(Color value)
		{
			GetComponent<MeshRenderer>().material.color = value;
		}
	}
}