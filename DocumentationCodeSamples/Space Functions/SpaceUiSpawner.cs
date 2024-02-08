using CavrnusSdk.API;
using UnityEngine;

public class SpaceUiSpawner : MonoBehaviour
{
    public GameObject SpaceMenuPrefab;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => GameObject.Instantiate(SpaceMenuPrefab));
    }
}
