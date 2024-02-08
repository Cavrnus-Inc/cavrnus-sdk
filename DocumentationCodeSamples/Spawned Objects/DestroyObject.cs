using CavrnusSdk.API;
using CavrnusSdk;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public void DestroyMe()
    {
        if (GetComponent<CavrnusSpawnedObjectFlag>() == null)
            Debug.LogError("This object was not spawned via Cavrnus, cannot destroy it");

        CavrnusSpawnedObject spawnedObject = GetComponent<CavrnusSpawnedObjectFlag>().SpawnedObject;

        spawnedObject.DestroyObject();
    }
}