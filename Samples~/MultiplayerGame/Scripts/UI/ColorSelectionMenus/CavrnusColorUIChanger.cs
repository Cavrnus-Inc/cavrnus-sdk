using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusColorUIChanger : CavrnusColorCollectionUIBase
    {
        protected override void BindProperty()
        {
            Binding = SpaceConn.BindColorPropertyValue(ContainerName, StringName, OnServerValueUpdated);
        }

        private void OnServerValueUpdated(Color serverColor)
        {
            var serverData = ColorCollection.GetDataFromColor(serverColor);
            SetSelectedItem(serverData);
        }

        protected override void OnSelected(CavrnusColorCollection.ColorTextureInfo data)
        {
            SpaceConn?.PostColorPropertyUpdate(ContainerName, StringName, data.Color);
        }
    }
}