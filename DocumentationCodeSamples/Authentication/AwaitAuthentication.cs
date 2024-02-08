using CavrnusSdk.API;
using UnityEngine;

public class AwaitAuthentication : MonoBehaviour
{
    public GameObject SpacesMenuPrefab;

    void Start()
    {
        CavrnusFunctionLibrary.AwaitAuthentication(auth => OnAuth(auth));
    }

    private void OnAuth(CavrnusAuthentication auth)
    {
        //Instantiate the Spaces Menu now that we are logged in
        GameObject.Instantiate(SpacesMenuPrefab);
    }
}
