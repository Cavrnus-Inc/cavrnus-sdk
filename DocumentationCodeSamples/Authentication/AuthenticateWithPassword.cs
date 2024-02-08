using CavrnusSdk.API;
using TMPro;
using UnityEngine;

public class AuthenticateWithPassword : MonoBehaviour
{
    public TMP_InputField UserEmailInput;
    public TMP_InputField UserPasswordInput;

    private const string MyServer = "cavrnus.cavrn.us";

    public void Authenticate()
    {
        CavrnusFunctionLibrary.AuthenticateWithPassword(MyServer, UserEmailInput.text, UserPasswordInput.text, 
            auth => OnAuthComplete(auth), error => OnAuthFailure(error));
    }

    private void OnAuthComplete(CavrnusAuthentication auth)
    {
        Debug.Log("Authentication complete!  Welcome to Cavrnus!");
    }

    //Generally an invalid email or password
    private void OnAuthFailure(string error)
    {
        Debug.LogError("Failed to login: " + error);
    }
}
