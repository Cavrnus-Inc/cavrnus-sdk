using CavrnusSdk.API;
using UnityEngine;

public class ManageObjectColor : MonoBehaviour
{
    public Material MyMaterial;

    private const string ContainerName = "MyMaterial";
    private const string PropertyName = "color";

    private CavrnusSpaceConnection spaceConn;

    private void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => OnConnectedToSpace(spaceConn));
    }

    private void OnConnectedToSpace(CavrnusSpaceConnection spaceConn)
    {
        this.spaceConn = spaceConn;

        //Compare my state to the server, and post an update if out-of-sync
        Color serverPropertyValue = spaceConn.GetColorPropertyValue(ContainerName, PropertyName);

        if (!serverPropertyValue.Equals(MyMaterial.color))
            spaceConn.PostColorPropertyUpdate(ContainerName, PropertyName, MyMaterial.color);


        spaceConn.DefineColorPropertyDefaultValue(ContainerName, PropertyName, MyMaterial.color);

        spaceConn.BindColorPropertyValue(ContainerName, PropertyName, c => SetMyMaterialColor(c));
    }

    public void SetMyMaterialColor(Color color)
    {
        MyMaterial.color = color;
    }

    public void SetToNewColor(Color newColor)
    {
        spaceConn.PostColorPropertyUpdate(ContainerName, PropertyName, newColor);
    }

    private CavrnusLivePropertyUpdate<Color> liveColorUpdate = null;

    //Recv this many times as the user drags their mouse around the UI
    public void UseColorWheel(Color color)
    {
        if (liveColorUpdate == null)
        {
            liveColorUpdate = spaceConn.BeginTransientColorPropertyUpdate(ContainerName, PropertyName, color);
        }
        else
        {
            liveColorUpdate.UpdateWithNewData(color);
        }
    }

    //The user is done interacting with the color wheel
    public void ReleaseColorWheel(Color color)
    {
        if(liveColorUpdate != null)
        {
            liveColorUpdate.Finish();
            liveColorUpdate = null;
        }
    }

    //The color wheel closed and any live changes should reset back to their previous value
    public void CancelColorWheel()
    {
        if (liveColorUpdate != null)
        {
            liveColorUpdate.Cancel();
            liveColorUpdate = null;
        }
    }
}
