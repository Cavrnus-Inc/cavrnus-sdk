using CavrnusSdk.API;
using UnityEngine;

public class ExitSpaceButton : MonoBehaviour
{
    private CavrnusSpaceConnection spaceConn;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => this.spaceConn = spaceConn);
    }

    public void ExecuteExitSpace()
    {
        if(spaceConn == null)
        {
            Debug.LogError("Not yet in a space that can be exited");
            return;
        }

        spaceConn.ExitSpace();
    }
}
