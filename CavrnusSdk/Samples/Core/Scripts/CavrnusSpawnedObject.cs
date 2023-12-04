using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusSpawnedObject : MonoBehaviour
	{
		public string SpawnedId = null;
		public string CreationOpId = null;

		public void Init(string spawnedId, string creationOpId)
		{
			SpawnedId = spawnedId;
			CreationOpId = creationOpId;
		}
	}
}
