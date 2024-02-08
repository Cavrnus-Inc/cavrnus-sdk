using CavrnusSdk.API;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    private CavrnusSpaceConnection spaceConn;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => this.spaceConn = spaceConn);
    }

    public void SpawnNpc()
    {
        if (spaceConn == null)
            Debug.LogError("Cannot spawn object, not yet connected to a space");

        string npcContainerName = spaceConn.SpawnObject("NpcPrefabUniqueID");

        //Flip the object 180 degrees and place it at the origin
        spaceConn.PostTransformPropertyUpdate(npcContainerName, "transform", Vector3.zero, new Vector3(0, 180, 0), Vector3.one);
    }
}
