using UnityEngine;

namespace CavrnusSdk
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSync<T> : MonoBehaviour, ICavrnusValueSync<T>
	{
		[Header("The ID of the Property Value you are modifying:")]
		public string PropertyName = "Property";

		public bool SendMyChanges = true;

		public abstract T GetValue();
		public abstract void SetValue(T value);

		private CavrnusDisplayProperty<T> displayer;
		private CavrnusPostSynchronizedProperty<T> sender;

		public string PropName => PropertyName;
		public CavrnusPropertiesContainer Context => GetComponent<CavrnusPropertiesContainer>();

		void Start()
		{
			if (string.IsNullOrWhiteSpace(PropertyName))
				throw new System.Exception($"A PropertyName has not been assigned on object {gameObject.name}");

			displayer = new CavrnusDisplayProperty<T>(this);
			if (SendMyChanges) sender = new CavrnusPostSynchronizedProperty<T>(this);
		}

		private void OnDestroy()
		{
			displayer?.Dispose();
			if (sender != null) sender?.Dispose();
		}
	}

	public interface ICavrnusValueSync<T>
	{
		string PropName{ get; }
		CavrnusPropertiesContainer Context{ get; }
		T GetValue();
		void SetValue(T value);
	}
}