using CavrnusSdk.API;
using System.Collections.Generic;
using UnityEngine;

public class BindJoinableSpaces : MonoBehaviour
{
    public SpaceOptionUI SpaceUiPrefab;

    public Transform SpacesListParent;

    private Dictionary<CavrnusSpaceInfo, SpaceOptionUI> instantiatedSpaceOptions = new Dictionary<CavrnusSpaceInfo, SpaceOptionUI>();

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.BindJoinableSpaces(space => SpaceAdded(space), 
            space => SpaceUpdated(space), 
            space => SpaceRemoved(space));
    }

    private void SpaceAdded(CavrnusSpaceInfo space)
    {
        var spaceOption = GameObject.Instantiate(SpaceUiPrefab, SpacesListParent);
        spaceOption.Setup(space);
    }

    private void SpaceUpdated(CavrnusSpaceInfo space)
    {
        var oldSpaceOption = instantiatedSpaceOptions[space];
        GameObject.Destroy(oldSpaceOption);

        var spaceOption = GameObject.Instantiate(SpaceUiPrefab, SpacesListParent);
        spaceOption.Setup(space);
    }

    private void SpaceRemoved(CavrnusSpaceInfo space)
    {
        var oldSpaceOption = instantiatedSpaceOptions[space];
        GameObject.Destroy(oldSpaceOption);
    }
}
