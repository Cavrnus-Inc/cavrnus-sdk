using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Editor
{
	public class CavrnusEditorHelpers
	{
		/*[MenuItem("Assets/Cavrnus/Setup Scene For Instant Link Join", false, 0)]
		public static void SetupCavrnusForInstantLinks()
		{

		}

		[MenuItem("Assets/Cavrnus/Setup Scene For Login", false, 10)]
		public static void SetupCavrnusForLogin()
		{

		}*/

		[MenuItem("Assets/Cavrnus/Make Selection Spawnable", false, 100)]
		public static void MakeSelectionSpawnable()
		{
			var spawnablePrefabs =
				AssetDatabase.LoadAssetAtPath<CavrnusSpawnablePrefabsLookup>(
					"Assets/CavrnusSdk/Cavrnus Spawnable Prefabs Lookup.asset");
			if (spawnablePrefabs == null) {
				throw new System.Exception(
					"Could not find CavrnusSpawnablePrefabsLookup at Assets/CavrnusSdk/Cavrnus Spawnable Prefabs Lookup.asset.  If you have moved or renamed it then this Editor Script will not work.");
			}

			foreach (GameObject obj in Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets)) {
				if (spawnablePrefabs.SpawnablePrefabs.Contains(obj.GetComponent<CavrnusSpawnablePrefab>())) {
					Debug.Log($"{obj} is already registered as a Spawnable Prefab");
					continue;
				}

				if (obj.GetComponent<CavrnusSpawnablePrefab>() == null) {
					obj.AddComponent<CavrnusSpawnablePrefab>();
					EditorUtility.SetDirty(obj);
				}

				string spawnablePrefaDesiredId = obj.name;

				//Fuck it, it's an Editor-time Script.  Doesn't need to be super clever/efficient
				//This will increase in cost as users add more and more prefabs sharing the sane name
				//So if anybody has 1 million prefabs named Cube and wants all of them in Cavrnus, we'll write something cleverer
				for (int i = 0; i < Int32.MaxValue; i++) {
					string nameToUse = spawnablePrefaDesiredId + "_" + i;
					if (i == 0) nameToUse = spawnablePrefaDesiredId;

					if (!spawnablePrefabs.SpawnablePrefabs.Any(sp => sp.UniqueIdentifier.Equals(nameToUse))) {
						obj.GetComponent<CavrnusSpawnablePrefab>().UniqueIdentifier = nameToUse;
						EditorUtility.SetDirty(obj);

						spawnablePrefabs.SpawnablePrefabs.Add(obj.GetComponent<CavrnusSpawnablePrefab>());
						EditorUtility.SetDirty(spawnablePrefabs);
						break;
					}
				}
			}
		}
	}
}