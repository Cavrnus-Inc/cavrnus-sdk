using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusCore : MonoBehaviour
	{
		public static CavrnusCore Instance{
			get {
				if (instance == null) instance = GameObject.Find("Cavrnus Core")?.GetComponent<CavrnusCore>();

				if (instance == null)
					throw new System.Exception(
						"Cannot find an instance of Cavrnus Core in the scene.  Did you remove or rename it?");

				return instance;
			}
		}

		private static CavrnusCore instance;

		public CavrnusSpawnablePrefabsLookup SpawnablePrefabs;
		public CavrnusSettings CavrnusSettings;
	}
}