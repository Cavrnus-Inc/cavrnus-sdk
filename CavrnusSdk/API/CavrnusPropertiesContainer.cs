using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusPropertiesContainer : MonoBehaviour
	{
		[Header(
			"The ID of the Property Container this value lives in.\nNote that two scripts referencing the same Container ID will get/set the same value.")]
		[SerializeField]
		private string UniqueContainerName;

		public void SetContainerName(string containerName) { UniqueContainerName = containerName; }

		public string[] UniqueContainerPath{
			get {
				//If spawned at runtime, use the spawned parents prefix
				string parentPrefix = null;
				if (gameObject.GetComponentInParent<CavrnusSpawnedObject>() != null) {
					parentPrefix = gameObject.GetComponentInParent<CavrnusSpawnedObject>().SpawnedId;
				}
				else if (gameObject.GetComponent<CavrnusSpawnedObject>() != null) {
					parentPrefix = gameObject.GetComponent<CavrnusSpawnedObject>().SpawnedId;
				}

				List<string> path = new List<string>();

				if (!string.IsNullOrWhiteSpace(UniqueContainerName))
					path = UniqueContainerName.Split('/').ToList();
				else
					path = GetGameObjectPath(gameObject);

				if (!string.IsNullOrWhiteSpace(parentPrefix)) path.Insert(0, parentPrefix);

				return path.ToArray();
			}
		}

		private static List<string> GetGameObjectPath(GameObject obj)
		{
			var path = new List<string>();
			path.Insert(0, obj.name);
			while (obj.transform.parent != null &&
			       obj.transform.parent.GetComponent<CavrnusSpawnablePrefab>() != null) {
				obj = obj.transform.parent.gameObject;
				path.Insert(0, obj.name);
			}

			return path;
		}
	}
}