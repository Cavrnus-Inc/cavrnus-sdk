using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk
{
	public class CavrnusSpawnedObjectFlag : MonoBehaviour
	{
		public CavrnusSpawnedObject SpawnedObject;

		public void Init(CavrnusSpawnedObject spawnedObject)
		{
			SpawnedObject = spawnedObject;
		}
	}
}
