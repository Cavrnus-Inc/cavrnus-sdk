using System.Collections.Generic;
using UnityEngine;
using System;

namespace CavrnusSdk.PropertySynchronizers
{
	public class CavrnusPropertiesContainer : MonoBehaviour
	{
		[Header(
			"The ID of the Property Container this value lives in.\nNote that two scripts referencing the same Container ID will get/set the same value.")]
		[SerializeField]
		private string UniqueContainerName;

		public void PrefixContainerName(string prefix) 
		{
			if(string.IsNullOrWhiteSpace(UniqueContainerName))
				UniqueContainerName = prefix;
			else
				UniqueContainerName = $"{prefix}/{UniqueContainerName}";
		}

		public bool IsUserProperty()
		{
			return UniqueContainerName.StartsWith("/users/");
		}

		public string UniqueContainerPath{
			get {
				//If spawned at runtime, use the spawned parents prefix
				if (gameObject.GetComponent<CavrnusSpawnedObjectFlag>() != null) {
					return gameObject.GetComponent<CavrnusSpawnedObjectFlag>().SpawnedObject.PropertiesContainerName;
				}				

				if (!string.IsNullOrWhiteSpace(UniqueContainerName))
					return UniqueContainerName;
				else
					return String.Join("/", GetGameObjectPath(gameObject));

            }
		}

		private static List<string> GetGameObjectPath(GameObject obj)
		{
			var path = new List<string>();
			path.Insert(0, obj.name);
			while (obj.transform.parent != null &&
			       obj.transform.parent.GetComponent<CavrnusSpawnedObjectFlag>() != null) {
				obj = obj.transform.parent.gameObject;
				path.Insert(0, obj.name);
			}

			return path;
		}
	}
}