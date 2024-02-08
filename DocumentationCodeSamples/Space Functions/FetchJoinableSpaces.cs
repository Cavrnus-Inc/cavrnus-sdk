using CavrnusSdk.API;
using UnityEngine;

public class FetchJoinableSpaces : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAuthentication(auth => FetchMySpaces());
    }

    private void FetchMySpaces()
    {
        CavrnusFunctionLibrary.FetchJoinableSpaces(spaces =>
        {
            //Attempt to join the space named "DEMO"
            foreach (var space in spaces)
            {
                if (space.Name == "DEMO")
                {
                    CavrnusFunctionLibrary.JoinSpace(space.Id, spaceConn => { }, err => { });
                    return;
                }
            }

            Debug.LogError("No available space named \"DEMO\"");
        });
    }
}
