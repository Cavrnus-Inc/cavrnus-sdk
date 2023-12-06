using UnityEditor;

namespace CavrnusSdk.Editor
{
	public static class ExportPackage
	{
		[MenuItem("Export/UnityPluginPackage")]
		public static void BuildPluginPackage()
		{
			BuildExportPackage("Cavrnus Metaverse Connector.unitypackage");
		}

		private static void BuildExportPackage(string dest)
		{
			AssetDatabase.ExportPackage(new[] { "Assets/CavrnusSdk"}, dest, ExportPackageOptions.Recurse);
		}
	}
}