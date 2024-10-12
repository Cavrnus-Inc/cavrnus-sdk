#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace CavrnusSdk.Setup.Editor
{
	[CustomEditor(typeof(CavrnusSpatialConnector))]
	public class CavrnusSpatialConnectorEditor : UnityEditor.Editor
	{
		SerializedProperty MyServer;

		SerializedProperty AuthenticationMethod;
		SerializedProperty MemberLoginMethod;
		SerializedProperty GuestLoginMethod;

		SerializedProperty GuestJoinMenu;
		SerializedProperty MemberLoginMenu;

		SerializedProperty MemberEmail;
		SerializedProperty MemberPassword;

		SerializedProperty GuestName;

		SerializedProperty SaveUserToken;
		SerializedProperty SaveGuestToken;

		SerializedProperty SpaceJoinMethod;

		SerializedProperty SpacesListMenu;

		SerializedProperty AutomaticSpaceJoinId;

		SerializedProperty LoadingMenus;

		SerializedProperty SpaceMenus;

		SerializedProperty RemoteUserAvatar;

		SerializedProperty UiCanvas;

        SerializedProperty SpawnableObjects;

        SerializedProperty AdditionalSettings;
        
        SerializedProperty SpawnRemoteAvatars;
        SerializedProperty ShowLocalUser;
        
        private void OnEnable()
		{
			MyServer = serializedObject.FindProperty(nameof(MyServer));

			AuthenticationMethod = serializedObject.FindProperty(nameof(AuthenticationMethod));
			MemberLoginMethod = serializedObject.FindProperty(nameof(MemberLoginMethod));
			GuestLoginMethod = serializedObject.FindProperty(nameof(GuestLoginMethod));

			GuestJoinMenu = serializedObject.FindProperty(nameof(GuestJoinMenu));
			MemberLoginMenu = serializedObject.FindProperty(nameof(MemberLoginMenu));
			MemberEmail = serializedObject.FindProperty(nameof(MemberEmail));
			MemberPassword = serializedObject.FindProperty(nameof(MemberPassword));
			GuestName = serializedObject.FindProperty(nameof(GuestName));

			SaveUserToken = serializedObject.FindProperty(nameof(SaveUserToken));
			SaveGuestToken = serializedObject.FindProperty(nameof(SaveGuestToken));

			SpaceJoinMethod = serializedObject.FindProperty(nameof(SpaceJoinMethod));
			SpacesListMenu = serializedObject.FindProperty(nameof(SpacesListMenu));
			AutomaticSpaceJoinId = serializedObject.FindProperty(nameof(AutomaticSpaceJoinId));

			LoadingMenus = serializedObject.FindProperty(nameof(LoadingMenus));
			SpaceMenus = serializedObject.FindProperty(nameof(SpaceMenus));
			UiCanvas = serializedObject.FindProperty(nameof(UiCanvas));

			SpawnableObjects = serializedObject.FindProperty(nameof(SpawnableObjects));
            AdditionalSettings = serializedObject.FindProperty(nameof(AdditionalSettings));
            
            RemoteUserAvatar = serializedObject.FindProperty(nameof(RemoteUserAvatar));
            SpawnRemoteAvatars = serializedObject.FindProperty(nameof(SpawnRemoteAvatars));
            ShowLocalUser = serializedObject.FindProperty(nameof(ShowLocalUser));
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
				Application.OpenURL("https://cavrnus.atlassian.net/servicedesk/customer/portal/1/article/827457539");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("First please specify the server you are using (ex: \"yourcompany.cavrn.us\")", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(MyServer);
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField("If you don't have an account, contact us at sales@cavrnus.com");
			// if (EditorGUILayout.LinkButton("Contact Us"))
			// {
			// 	Application.OpenURL("https://www.cavrnus.com/");
			// }
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("How do you want your users to Authenticate?", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(AuthenticationMethod);
			
			if (AuthenticationMethod.enumValueFlag == 0) { //JoinAsGuest
				EditorGUILayout.PropertyField(GuestLoginMethod);
				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (GuestLoginMethod.enumValueFlag == 0) { //EnterUserLoginCredentials
					EditorGUILayout.LabelField("Automatically joins as a guest with the name below:");
					EditorGUILayout.PropertyField(GuestName);
				}
				else if (GuestLoginMethod.enumValueFlag == 1) { //PromptUserToLogin
					EditorGUILayout.LabelField("Spawns a UI menu that lets a guest input their name:");
					EditorGUILayout.PropertyField(GuestJoinMenu);
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Should the guest auth token be stored on Disk?", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("If so, it will bypass the login step on subsequent joins until they logout or the token expires.");
					EditorGUILayout.PropertyField(SaveGuestToken);
				}
			}
			else if (AuthenticationMethod.enumValueFlag == 1) { //JoinAsMember
				EditorGUILayout.PropertyField(MemberLoginMethod);
				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (MemberLoginMethod.enumValueFlag == 0) { //EnterUserLoginCredentials
					EditorGUILayout.PropertyField(MemberEmail);
					EditorGUILayout.PropertyField(MemberPassword);
				}
				else if (MemberLoginMethod.enumValueFlag == 1) { //PromptUserToLogin
					EditorGUILayout.LabelField("Spawns a UI menu that lets a user log into their account:");
					EditorGUILayout.PropertyField(MemberLoginMenu);
					
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Should the users auth token be stored on Disk?", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("If so, it will bypass the login step on subsequent joins until they logout or the token expires.");
					EditorGUILayout.PropertyField(SaveUserToken);
				}
				
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("User accounts can be created and managed in the Web Console:");
				WebConsoleButton();
				EditorGUILayout.EndHorizontal();
			}
			else if (AuthenticationMethod.enumValueFlag == 2) //None
			{
				EditorGUILayout.LabelField("Cavrnus Spatial Connector will establish singleton systems for you to call into, but no other setup will be performed.");
				serializedObject.ApplyModifiedProperties();
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("How do you want to join a space?", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(SpaceJoinMethod);
			
			var uiCanvasValue = UiCanvas.objectReferenceValue;

			if (SpaceJoinMethod.enumValueFlag == 0) //Automatic
			{
				EditorGUILayout.LabelField("Automatically use the given join config:");

				EditorGUILayout.PropertyField(AutomaticSpaceJoinId);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spaces can be created and managed from the Web Console:");
				WebConsoleButton();
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
				EditorGUILayout.LabelField("Cavrnus Spatial Connector will not attempt to spawn any UI or join a space automatically.");

				serializedObject.ApplyModifiedProperties();

				if(AuthenticationMethod.enumValueFlag == 3 || AuthenticationMethod.enumValueFlag == 1)//Uses UI
				{
					HandleCanvasRequirement(uiCanvasValue, true);
				}				

				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Would you like to show the local user to clients?", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(ShowLocalUser);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Would you like to use the default remote avatar display system?", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(SpawnRemoteAvatars);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			if (SpawnRemoteAvatars.boolValue) {
				EditorGUILayout.LabelField("What should be the remote user's Avatar?", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Leave blank to not spawn CoPresence.");
				EditorGUILayout.PropertyField(RemoteUserAvatar);
				
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
			}

            EditorGUILayout.LabelField("Which objects does your Application know how to spawn?", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(SpawnableObjects);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("For specific device settings, please refer to the documentation");
            if (EditorGUILayout.LinkButton("Settings"))
            {
	            Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/841187346/Cavrnus+on+Multiple+Devices");
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(AdditionalSettings);

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

		//TODO: Update this to be the console!!!
		private void WebConsoleButton()
		{
			if (EditorGUILayout.LinkButton("Web Console"))
			{				
				Application.OpenURL("https://console.dev.cavrn.us/spaces");

				/*if (string.IsNullOrEmpty(MyServer.stringValue))
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
				}*/
			}
		}
	}
}
#endif