using CavrnusSdk.API;
using UnityEngine;

public class SpaceLoadingUIManager : MonoBehaviour
{
    public GameObject SpaceLoadingIndicatorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceBeginLoading(spaceId =>
        {
            var loadingUi = GameObject.Instantiate(SpaceLoadingIndicatorPrefab);
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => GameObject.Destroy(loadingUi));
        });
    }
}
