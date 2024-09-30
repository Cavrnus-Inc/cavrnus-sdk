#if UNITY_EDITOR
using CavrnusSdk.Setup;
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
			var csc = GameObject.Find("Cavrnus Spatial Connector")?.GetComponent<CavrnusSpatialConnector>();
			if(csc == null)
			{
				Debug.LogError("No Cavrnus Spatial Connector found in your Scene.  Please select \"Cavrnus->Setup Scene for Cavrnus\" to create one.");
				return;
			}

			foreach (GameObject obj in Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets)) 
			{
				if (csc.SpawnableObjects.Any(sp => sp.Object == obj)) {
					Debug.Log($"{obj} is already registered as a Spawnable Object");
					continue;
				}

				string spawnablePrefaDesiredId = obj.name;

				// Brute force approach. Costly if user has 1 million prefabs named Cube and wants all of them in Cavrnus.
				// Can be inefficient for now...
				for (int i = 0; i < Int32.MaxValue; i++) {
					string nameToUse = spawnablePrefaDesiredId + "_" + i;
					if (i == 0) nameToUse = spawnablePrefaDesiredId;

					if (!csc.SpawnableObjects.Any(sp => sp.UniqueId.Equals(nameToUse))) 
					{
                        csc.SpawnableObjects.Add(new CavrnusSpatialConnector.CavrnusSpawnableObject() { Object = obj, UniqueId = nameToUse });

						break;
					}
				}
			}

			EditorUtility.SetDirty(csc);

        }
	}
}
#endif