using System;
using System.Collections;
using CavrnusCore;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers.CommonImplementations;
using CavrnusSdk.Setup;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Cavrnus.Tests
{
    public static class TestHelpers
    {
        public static string TestServer = "cav.dev.cavrn.us";
        public static string TestUser = " unittest@cavrn.us";
        public static string TestPassword = "Cavrnus";
        public static string TestSpaceJoinId = "UnitTestSpace";
        public static string CscPackagePath = "Assets/com.cavrnus.csc/CavrnusSdk/Runtime/Prefabs/Cavrnus Spatial Connector.prefab";
        public static string CanvasPackagePath = "Assets/com.cavrnus.csc/CavrnusSdk/Runtime/Prefabs/Cavrnus UI Canvas.prefab";

        public static CavrnusSpatialConnector GenericGuestSceneSetup(string testServer)
        {
			var connectorOb = SetupCavrnusComponents(testServer);
			connectorOb.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsGuest;
			connectorOb.GuestLoginMethod = CavrnusSpatialConnector.GuestLoginOptionEnum.EnterNameBelow;
			connectorOb.GuestName = "Unit Test Guest";

			return connectorOb;
        }

		public static CavrnusSpatialConnector GenericMemberSceneSetup(string testServer)
		{
			var connectorOb = SetupCavrnusComponents(testServer);
			connectorOb.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsMember;
			connectorOb.MemberLoginMethod = CavrnusSpatialConnector.MemberLoginOptionEnum.EnterMemberLoginCredentials;
			connectorOb.MemberEmail = TestUser;
			connectorOb.MemberPassword = TestPassword;

			return connectorOb;
		}

		public static CavrnusSpatialConnector SetupCavrnusComponents(string testServer)
        {
			PlayerPrefs.SetString("MemberCavrnusAuthToken", "");

			var mainCamera = new GameObject("Main Camera");
			mainCamera.AddComponent<AudioListener>();
			mainCamera.AddComponent<CavrnusLocalUserFlag>();
			var slt = mainCamera.AddComponent<SyncLocalTransform>();
			slt.PropertyName = "Transform";
			Selection.activeObject = mainCamera;

			//Get CSC Prefab
			var corePrefab = AssetDatabase.LoadAssetAtPath<CavrnusSpatialConnector>(CscPackagePath);
			var connectorOb = PrefabUtility.InstantiatePrefab(corePrefab) as CavrnusSpatialConnector;
			if (connectorOb == null)
			{
				Debug.LogError("Could not instantiate core prefab!");
				return null;
			}

			//Get Cav UI Canvas
			var canvasPrefab = AssetDatabase.LoadAssetAtPath<Canvas>(CanvasPackagePath);
			var canvasOb = PrefabUtility.InstantiatePrefab(canvasPrefab) as Canvas;
			if (canvasOb == null)
			{
				Debug.LogError("Could not instantiate canvas prefab!");
				return null;
			}

			connectorOb.UiCanvas = canvasOb;
			connectorOb.transform.SetAsFirstSibling();
			connectorOb.MyServer = testServer;
			connectorOb.AdditionalSettings.DisableVoice = true;
			connectorOb.AdditionalSettings.DisableAcousticEchoCancellation = true;
			connectorOb.AutomaticSpaceJoinId = TestSpaceJoinId;
			canvasOb.transform.SetAsFirstSibling();
			Selection.SetActiveObjectWithContext(connectorOb, connectorOb);

			return connectorOb;
		}
        
        public static void SceneTeardown()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name.ToLowerInvariant().Contains("runner"))
                    continue;

				GameObject.DestroyImmediate(obj);				
			}

			//It's in the Don'tDestroyOnLoad scene so we need to get it separately
			GameObject.DestroyImmediate(GameObject.Find("Cavrnus Spatial Connector"));

			CavrnusFunctionLibrary.ShutdownCavrnus();
		}
        
        public static IEnumerator AwaitSpaceConn(Action<CavrnusSpaceConnection> onSpaceConnected)
        {
            CavrnusSpaceConnection spaceConn = null;
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => spaceConn = sc);

            var count = 0;
            while (spaceConn == null && count < 200) {
                yield return new WaitForSeconds(.05f);
                
                count++;
            }

            onSpaceConnected(spaceConn);
        }
    }
}