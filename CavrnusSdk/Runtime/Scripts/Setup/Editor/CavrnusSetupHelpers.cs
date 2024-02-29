using UnityBase;
using UnityEditor;
using UnityEngine;
using CavrnusSdk.PropertySynchronizers.CommonImplementations;

namespace CavrnusSdk.Setup.Editor
{
	public static class CavrnusSetupHelpers
	{
		[MenuItem("Cavrnus/Setup Scene for Cavrnus", false, 0)]
		public static void SetupSceneForCavrnus()
	    {
			if(GameObject.Find("Cavrnus Spatial Connector") != null)
			{
				Debug.LogWarning("A Cavrnus Spatial Connector object already exists in your scene.  If you wish to replace it please delete it first.");
				return;
			}

			string cscPackagePath = "Packages/com.cavrnus.csc/CavrnusSdk/Runtime/Prefabs/Cavrnus Spatial Connector.prefab";
			string cscAssetsPath = "Assets/CavrnusSdk/Runtime/Prefabs/Cavrnus Spatial Connector.prefab";

            var corePrefab = AssetDatabase.LoadAssetAtPath<CavrnusSpatialConnector>(cscPackagePath);

			//For development project
			if(corePrefab == null)
				corePrefab = AssetDatabase.LoadAssetAtPath<CavrnusSpatialConnector>(cscAssetsPath);

            if (corePrefab == null)
			{
				Debug.LogError($"Cavrnus Spatial Connector prefab was not found at its expected location ({cscPackagePath}).  Please update or reinstall the Plugin to fix!");
				return;
			}

			var ob = PrefabUtility.InstantiatePrefab(corePrefab);

			(ob as CavrnusSpatialConnector).transform.SetAsFirstSibling();

			Selection.SetActiveObjectWithContext(ob, ob);

			string canvasPackagesPath = "Packages/com.cavrnus.csc/CavrnusSdk/Runtime/Prefabs/Cavrnus UI Canvas.prefab";
			string canvasAssetsPath = "Assets/CavrnusSdk/Runtime/Prefabs/Cavrnus UI Canvas.prefab";

            var canvasPrefab = AssetDatabase.LoadAssetAtPath<Canvas>(canvasPackagesPath);

            //For development project
            if (canvasPrefab == null)
				canvasPrefab = AssetDatabase.LoadAssetAtPath<Canvas>(canvasAssetsPath);

            if (canvasPrefab == null)
            {
                Debug.LogError($"Cavrnus UI Canvas prefab was not found at its expected location ({canvasPackagesPath}).  Please update or reinstall the Plugin to fix!");
                return;
            }

            var canvasOb = PrefabUtility.InstantiatePrefab(canvasPrefab);
            (ob as CavrnusSpatialConnector).UiCanvas = (canvasOb as Canvas);

			Debug.Log("Cavrnus UI will spawn under the Cavrnus UI Canvas object.  To change this, select your desired canvas and click \"Cavrnus->Spawn Cavrnus UI in Selected Canvas\"");
		}

        [MenuItem("Cavrnus/Spawn Cavrnus UI in Selected Canvas", false, 10)]
		public static void SpawnCavrnusUiInSelectedCanvas()
		{
            if (GameObject.Find("Cavrnus Spatial Connector") == null)
            {
                Debug.LogError("No Cavrnus Spatial Connector found in scene.  Please select \"Cavrnus->Setup Scene for Cavrnus\" before configuring UI options.");
                return;
            }

			if(Selection.activeGameObject?.GetComponent<Canvas>() == null)
			{
                Debug.LogError("No Canvas selected in scene hierarchy.");
                return;
            }

            GameObject.Find("Cavrnus Spatial Connector").GetComponent<CavrnusSpatialConnector>().UiCanvas = Selection.activeGameObject?.GetComponent<Canvas>();

            if (GameObject.Find("Cavrnus UI Canvas") != null)
            {
				GameObject.DestroyImmediate(GameObject.Find("Cavrnus UI Canvas"));
                Debug.Log("Destroying unused Cavrnus UI Canvas object.");
                return;
            }
        }

        [MenuItem("Cavrnus/Set Selected Object As Local User", false, 10)]
		public static void SetSelectedObjectAsLocalUser()
		{
			if (Selection.activeGameObject == null)
			{
				Debug.LogError("No object has been selected to set as the local user");
				return;
			}

			if (Selection.activeGameObject.GetComponent<CavrnusLocalUserFlag>() != null)
			{
				Debug.LogWarning("Selected object is already configured to be the Local User. No further action is needed.");
			}
			else if (GameObject.FindObjectOfType<CavrnusLocalUserFlag>() != null)
			{
				Debug.LogError($"{GameObject.FindObjectOfType<CavrnusLocalUserFlag>().name} has already been set up as the Local User. There can be only one!");
				return;
			}
			else
			{
				Selection.activeGameObject.AddComponent<CavrnusLocalUserFlag>();
			}

			if (Selection.activeGameObject.GetComponent<SyncLocalTransform>() == null)
			{
				Debug.Log("Automatically adding a Sync Transform component to the local user, so that your CoPresence is sent to other users.");

				var st = Selection.activeGameObject.AddComponent<SyncLocalTransform>();
				st.PropertyName = "Transform";
			}
		}
	}
}