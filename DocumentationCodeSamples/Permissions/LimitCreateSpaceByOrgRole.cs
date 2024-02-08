using CavrnusSdk.API;
using UnityEngine.UI;
using UnityEngine;

public class LimitCreateSpaceByOrgRole : MonoBehaviour
{
    public Button CreatSpaceButton;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.BindGlobalPolicy("canCreateSpaces", allowed => ButtonPermissionsChanged(allowed));
    }

    public void ButtonPermissionsChanged(bool allowed)
    {
        CreatSpaceButton.gameObject.SetActive(allowed);
    }
}
