using UnityBase;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Editor
{
	public static class CavrnusSetupHelpers
	{
		[MenuItem("Cavrnus/Setup Space for Cavrnus", false, 0)]
		public static void SetupSpaceForCavrnus()
	    {
			if(GameObject.Find("Cavrnus Core") != null)
			{
				Debug.LogWarning("A Cavrnus Core object already exists in your scene.  If you wish to replace it please delete it first.");
				return;
			}

			var corePrefab = AssetDatabase.LoadAssetAtPath<CavrnusCore>("Assets/Samples/Core/Prefabs/Cavrnus Core.prefab");

			if(corePrefab == null)
			{
				Debug.LogError("Cavrnus Core prefab was not found at its expected location (Assets/Samples/Core/Prefabs/Cavrnus Core.prefab).  Please update or reinstall the Plugin to fix!");
				return;
			}

			var ob = PrefabUtility.InstantiatePrefab(corePrefab);

			(ob as CavrnusCore).transform.SetAsFirstSibling();

			Selection.SetActiveObjectWithContext(ob, ob);

			//Find or setup the Canvas

			var existingCanvas = GameObject.FindObjectOfType<Canvas>();
			if (existingCanvas != null)
			{
				Debug.Log($"Found pre-existing at Canvas: {existingCanvas.transform.HierarchyToString()}.  Cavrnus UI will be spawned under this.  You can change this on the Cavrnus Core object.");
				(ob as CavrnusCore).UiCanvas = existingCanvas;
			}
			else
			{
				Debug.Log($"No Canvas found, instantiating a new Cavrnus Canvas to display UI.  You can change this on the Cavrnus Core object.");
				var canvasPrefab = AssetDatabase.LoadAssetAtPath<Canvas>("Assets/Samples/Core/PrefabsCavrnus Canvas.prefab");
				var canvasOb = PrefabUtility.InstantiatePrefab(canvasPrefab);
				(ob as CavrnusCore).UiCanvas = (canvasOb as Canvas);
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

			

			if (Selection.activeGameObject.GetComponent<CavrnusLocalUser>() != null)
			{
				Debug.LogWarning("Selected object is already configured to be the Local User.  No further action is needed.");
			}
			else if (GameObject.FindObjectOfType<CavrnusLocalUser>() != null)
			{
				Debug.LogError($"{GameObject.FindObjectOfType<CavrnusLocalUser>().name} has already been set up as the Local User.  There can be only one!");
				return;
			}
			else
			{
				Selection.activeGameObject.AddComponent<CavrnusLocalUser>();
			}

			if (Selection.activeGameObject.GetComponent<SyncTransform>() == null)
			{
				Debug.Log("Automatically adding a Sync Transform component to the local user, so that your CoPresence is sent to other users.");

				var st = Selection.activeGameObject.AddComponent<SyncTransform>();
				st.PropertyName = "transform";
			}
		}
	}
}