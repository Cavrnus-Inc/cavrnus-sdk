using CavrnusSdk.API;

namespace CavrnusSdk.MultiplayerGame
{
    public class CavrnusTextureUIChanger : CavrnusColorCollectionUIBase
    {
        protected override void BindProperty()
        {
            Binding = SpaceConn.BindStringPropertyValue(ContainerName, StringName, OnServerValueUpdated);
        }

        private void OnServerValueUpdated(string serverTexture)
        {
            if (string.IsNullOrWhiteSpace(serverTexture)) 
                return;
                    
            var serverData = ColorCollection.GetDataFromTextureName(serverTexture);
            SetSelectedItem(serverData);
        }

        protected override void OnSelected(CavrnusColorCollection.ColorTextureInfo data)
        {
            SpaceConn?.PostColorPropertyUpdate(ContainerName, StringName, data.Color);
        }
    }
}