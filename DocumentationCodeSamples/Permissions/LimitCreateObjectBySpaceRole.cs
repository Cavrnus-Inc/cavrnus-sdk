using CavrnusSdk.API;
using UnityEngine;
using UnityEngine.UI;

public class LimitCreateObjectBySpaceRole : MonoBehaviour
{
    public Button CreatObjectButton;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
        {
            spaceConn.BindSpacePolicy("canCreateObjects", allowed => ButtonPermissionsChanged(allowed));
        });        
    }

    public void ButtonPermissionsChanged(bool allowed)
    {
        CreatObjectButton.gameObject.SetActive(allowed);
    }
}
