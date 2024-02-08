using CavrnusSdk.API;
using UnityEngine;

public class AuthenticateAsGuest : MonoBehaviour
{
    private const string MyServer = "cavrnus.cavrn.us";

    private void Start()
    {
        CavrnusFunctionLibrary.AuthenticateAsGuest(MyServer, "Cavrnus Guest User",
            auth => OnAuthComplete(auth), error => OnAuthFailure(error));
    }

    private void OnAuthComplete(CavrnusAuthentication auth)
    {
        Debug.Log("Authentication complete!  Welcome to Cavrnus!");
    }

    private void OnAuthFailure(string error)
    {
        Debug.LogError("Failed to login: " + error);
    }
}
