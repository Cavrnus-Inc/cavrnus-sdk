using CavrnusSdk.API;
using UnityEngine;

public class JoinSpaceUiButton : MonoBehaviour
{
    public void JoinGivenSpace(CavrnusSpaceInfo space)
    {
        CavrnusFunctionLibrary.JoinSpace(space.Id, 
            spaceConn => Debug.Log("Successfully Joined: " + space.Name), 
            err => Debug.LogError("Failed to join space: " + err));
    }
}
