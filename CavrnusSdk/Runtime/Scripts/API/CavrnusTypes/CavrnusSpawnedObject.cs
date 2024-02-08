namespace CavrnusSdk.API
{
	public class CavrnusSpawnedObject
	{
		public string PropertiesContainerName;

		internal string CreationOpId;

		internal CavrnusSpaceConnection spaceConnection;

		internal CavrnusSpawnedObject(string propsContainerName, string creationOpId, CavrnusSpaceConnection spaceConn)
		{
			spaceConnection = spaceConn;
			PropertiesContainerName = propsContainerName;
			CreationOpId = creationOpId;
		}
	}
}