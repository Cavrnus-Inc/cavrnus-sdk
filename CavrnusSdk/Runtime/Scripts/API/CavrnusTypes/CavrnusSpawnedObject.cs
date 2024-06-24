using UnityEngine;

namespace CavrnusSdk.API
{
	public class CavrnusSpawnedObject
	{
		public string PropertiesContainerName;
		public GameObject SpawnedObjectInstance;

		internal string CreationOpId;

		internal CavrnusSpaceConnection spaceConnection;

		internal CavrnusSpawnedObject(string propsContainerName, GameObject ob, string creationOpId, CavrnusSpaceConnection spaceConn)
		{
			spaceConnection = spaceConn;
			PropertiesContainerName = propsContainerName;
			SpawnedObjectInstance = ob;
			CreationOpId = creationOpId;
		}
	}
}