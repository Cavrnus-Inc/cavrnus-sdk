using UnityEngine;
using UnityEditor;

namespace CavrnusSdk.Editor
{
	[CustomEditor(typeof(CavrnusCore))]
	public class CavrnusCoreEditor : UnityEditor.Editor
	{
		SerializedProperty MyServer;

		SerializedProperty AuthenticationMethod;

		SerializedProperty ManualLoginMenu;

		SerializedProperty AutomaticLoginEmail;
		SerializedProperty AutomaticLoginPassword;

		SerializedProperty ManualGuestJoinMenu;

		SerializedProperty AutomaticGuestJoinUsername;

		SerializedProperty SaveUserToken;

		SerializedProperty SpaceJoinMethod;

		SerializedProperty SpacesListMenu;

		SerializedProperty AutomaticSpaceJoinId;

		SerializedProperty LoadingMenus;

		SerializedProperty SpaceMenus;

		SerializedProperty RemoteUserAvatar;

		SerializedProperty UiCanvas;

		void OnEnable()
		{
			MyServer = serializedObject.FindProperty("MyServer");

			AuthenticationMethod = serializedObject.FindProperty("AuthenticationMethod");

			ManualLoginMenu = serializedObject.FindProperty("ManualLoginMenu");

			AutomaticLoginEmail = serializedObject.FindProperty("AutomaticLoginEmail");
			AutomaticLoginPassword = serializedObject.FindProperty("AutomaticLoginPassword");

			ManualGuestJoinMenu = serializedObject.FindProperty("ManualGuestJoinMenu");

			AutomaticGuestJoinUsername = serializedObject.FindProperty("AutomaticGuestJoinUsername");

			SaveUserToken = serializedObject.FindProperty("SaveUserToken");

			SpaceJoinMethod = serializedObject.FindProperty("SpaceJoinMethod");

			SpacesListMenu = serializedObject.FindProperty("SpacesListMenu");

			AutomaticSpaceJoinId = serializedObject.FindProperty("AutomaticSpaceJoinId");

			LoadingMenus = serializedObject.FindProperty("LoadingMenus");

			SpaceMenus = serializedObject.FindProperty("SpaceMenus");

			RemoteUserAvatar = serializedObject.FindProperty("RemoteUserAvatar");

			UiCanvas = serializedObject.FindProperty("UiCanvas");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorStyles.label.wordWrap = true;
			EditorStyles.boldLabel.wordWrap = true;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Welcome to Cavrnus!", EditorStyles.boldLabel);
			if (EditorGUILayout.LinkButton("[Documentation]"))
			{
				Application.OpenURL("https://www.cavrnus.com/");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("First please specify the server you are using (ex: \"yourcompany.cavrn.us\")", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(MyServer);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("If you don't have a server yet:");
			if (EditorGUILayout.LinkButton("Register a New Domain"))
			{
				Application.OpenURL("https://www.cavrnus.com/");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("How do you want your users to Authenticate?", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(AuthenticationMethod);

			if(AuthenticationMethod.enumValueFlag == 0) //GuestJoinAutomatic
			{
				EditorGUILayout.LabelField("Automatically creates a simple guest account and logs in as that:");

				EditorGUILayout.PropertyField(AutomaticGuestJoinUsername);
			}
			else if (AuthenticationMethod.enumValueFlag == 1) //GuestJoinManual
			{
				EditorGUILayout.LabelField("Spawns a UI menu that lets a user input a username and create a guest account:");

				EditorGUILayout.PropertyField(ManualGuestJoinMenu);
			}
			else if (AuthenticationMethod.enumValueFlag == 2) //LoginAutomatic
			{
				EditorGUILayout.LabelField("Automatically logs the user into a pre-defined account:");

				EditorGUILayout.PropertyField(AutomaticLoginEmail);
				EditorGUILayout.PropertyField(AutomaticLoginPassword);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("User accounts can be created and managed in the Admin Portal:");
				AdminPortalButton();
				EditorGUILayout.EndHorizontal();
			}
			else if (AuthenticationMethod.enumValueFlag == 3) //LoginManual
			{
				EditorGUILayout.LabelField("Spawns a UI menu that lets a user log into their account:");

				EditorGUILayout.PropertyField(ManualLoginMenu);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("User accounts can be created and managed in the Admin Portal:");
				AdminPortalButton();
				EditorGUILayout.EndHorizontal();
			}
			else if (AuthenticationMethod.enumValueFlag == 4) //None
			{
				EditorGUILayout.LabelField("Cavrnus Core will establish singleton systems for you to call into, but no other setup will be performed.");

				serializedObject.ApplyModifiedProperties();

				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Should the token be stored on Disk?", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("If so it will bypass the login step on subsequent joins until they logout or the token expires.");
			EditorGUILayout.PropertyField(SaveUserToken);

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("How do you want to join a space?", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(SpaceJoinMethod);
			
			var uiCanvasValue = UiCanvas.objectReferenceValue;

			if (SpaceJoinMethod.enumValueFlag == 0) //Automatic
			{
				EditorGUILayout.LabelField("Automatically join the given space:");

				EditorGUILayout.PropertyField(AutomaticSpaceJoinId);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spaces can be created and managed from the Admin Portal:");
				AdminPortalButton();
				EditorGUILayout.EndHorizontal();
				
				// Auto Auth
				if (AuthenticationMethod.enumValueFlag == 0 || AuthenticationMethod.enumValueFlag == 2)
					HandleCanvasRequirement(uiCanvasValue, false);
				
				// Manual Auth
				if (AuthenticationMethod.enumValueFlag == 1 || AuthenticationMethod.enumValueFlag == 3)
					HandleCanvasRequirement(uiCanvasValue, true);
			}
			else if (SpaceJoinMethod.enumValueFlag == 1) //SpacesList
			{
				EditorGUILayout.LabelField("Spawns a UI menu that lets a user select a space to join from a list of ones available to them:");

				EditorGUILayout.PropertyField(SpacesListMenu);

				HandleCanvasRequirement(uiCanvasValue, true);
			}
			else if (SpaceJoinMethod.enumValueFlag == 2) //None
			{
				EditorGUILayout.LabelField("Cavrnus Core will not attempt to spawn any UI or join a space automatically.");

				serializedObject.ApplyModifiedProperties();

				if(AuthenticationMethod.enumValueFlag == 3 || AuthenticationMethod.enumValueFlag == 1)//Uses UI
				{
					HandleCanvasRequirement(uiCanvasValue, true);
				}				

				serializedObject.ApplyModifiedProperties();

				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("What should be the remote user's Avatar?", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Leave blank to not spawn CoPresence.");
			EditorGUILayout.PropertyField(RemoteUserAvatar);
			
			serializedObject.ApplyModifiedProperties();
		}

		private void HandleCanvasRequirement(Object uiCanvasValue, bool required)
		{
			UiCanvasPicker();
			
			if (uiCanvasValue == null) 
			{
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
		
				var boxStyle = new GUIStyle(GUI.skin.box) {
					fontStyle = FontStyle.Bold,
					padding = new RectOffset(20, 20, 20, 20)
				};

				var prevColor = GUI.color;
				GUI.color = required ? new Color(1f, 0.5f, 0.5f) : new Color(1f, 1f, 0.5f);

				var msg = required ? "This mode needs a canvas in order to display the UI." : "No UI will be displayed because you have deselected the Canvas.";
				
				GUILayout.Box(msg, boxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				
				GUI.color = prevColor;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			else {
				UIAssignmentFields();
			}
		}

		private void UIAssignmentFields()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("What UI should spawn when you join the space?", EditorStyles.boldLabel);

			EditorGUILayout.LabelField("Removed when space loading is complete:");

			EditorGUILayout.PropertyField(LoadingMenus);

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Shows once you enter the space:");

			EditorGUILayout.PropertyField(SpaceMenus);
		}

		private void UiCanvasPicker()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("What Canvas should the UI spawn under?", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(UiCanvas);
			EditorGUILayout.Space();
		}

		private void AdminPortalButton()
		{
			if (EditorGUILayout.LinkButton("Admin Portal"))
			{
				if (string.IsNullOrEmpty(MyServer.stringValue))
				{
					//TODO: Pop something up in their faces
					Debug.LogError("No Server specified!");
				}
				else
				{
					string server = MyServer.stringValue;
					if (!server.StartsWith("https://"))
						server = "http://" + server + "/admin/spaces";

					Application.OpenURL(server);
				}
			}
		}
	}

}
