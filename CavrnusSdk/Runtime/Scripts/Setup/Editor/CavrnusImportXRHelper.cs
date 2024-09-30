#if UNITY_EDITOR

using CavrnusSdk.Editor;
using UnityEditor;

namespace CavrnusSdk.Setup
{
    public static class CavrnusImportXRHelper
    {
        [MenuItem("Tools/Cavrnus/XR/Import XR Packages")]
        public static void SetupCavrnusXRProject()
        {
            // Start the chain by installing the Cavrnus XR package
            CavrnusPackageSampleImporter.TryImportPackage("com.cavrnus.xr");
        }
        
        [MenuItem("Tools/Cavrnus/XR/Import Cavrnus XR Sample")]
        public static void SetupCavrnusXRSample()
        {
            CavrnusPackageSampleImporter.FindPackage("com.cavrnus.xr",package => {
                CavrnusPackageSampleImporter.TryImportSample(package, "VR");
            });
        }
        
        [MenuItem("Tools/Cavrnus/XR/Import Unity XR Sample")]
        public static void SetupUnitXRSample()
        {
            CavrnusPackageSampleImporter.FindPackage("com.unity.xr.interaction.toolkit",package => {
                CavrnusPackageSampleImporter.TryImportSample(package, "Starter Assets");
            });
        }
    }
}
#endif