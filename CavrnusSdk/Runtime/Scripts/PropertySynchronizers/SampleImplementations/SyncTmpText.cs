using TMPro;
using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncTmpText : CavrnusStringPropertySynchronizer
	{
		[Header("The text component you want to update")]
		public TMP_Text TextComponent;

		public override string GetValue() { return TextComponent.text; }

		public override void SetValue(string value) { TextComponent.text = value; }
	}
}