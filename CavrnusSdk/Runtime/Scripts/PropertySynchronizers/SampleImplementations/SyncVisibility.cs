namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
	public class SyncVisibility : CavrnusBooleanPropertySynchronizer
	{
		public override bool GetValue() { return gameObject.activeSelf; }

		public override void SetValue(bool value) { gameObject.SetActive(value); }

		private void Reset() { PropertyName = "Visible"; }
	}
}