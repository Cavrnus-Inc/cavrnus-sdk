using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncMaterialSharedColor : CavrnusValueSyncColor
	{
		public override Color GetValue()
		{
			return GetComponent<MeshRenderer>().sharedMaterial.color;
		}

		public override void SetValue(Color value)
		{
			GetComponent<MeshRenderer>().sharedMaterial.color = value;
		}
	}
}