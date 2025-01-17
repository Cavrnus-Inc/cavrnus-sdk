using System.Collections.Generic;
using System.Threading.Tasks;
using CavrnusCore;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.Editor.PropertyDrawers;
using UnityEngine.Serialization;

namespace CavrnusSdk.Setup
{
	public class CavrnusSpatialConnector : MonoBehaviour
	{
		public string MyServer;

		public Canvas UiCanvas;

		public GameObject MyServerMenu;

		public enum AuthenticationOptionEnum
		{
			JoinAsGuest = 0,
			JoinAsMember = 1,
			Custom = 2,
		}
		
		public enum MemberLoginOptionEnum
		{
			EnterMemberLoginCredentials = 0,
			PromptMemberToLogin = 1,
		}
		
		public enum GuestLoginOptionEnum
		{
			EnterNameBelow = 0,
			PromptToEnterName = 1,
		}

		public AuthenticationOptionEnum AuthenticationMethod;
		
		public MemberLoginOptionEnum MemberLoginMethod;
		public GuestLoginOptionEnum GuestLoginMethod;

		public GameObject GuestJoinMenu;
		public GameObject MemberLoginMenu;

		public string MemberEmail;
		
		[PasswordField]
		public string MemberPassword;

		public string GuestName;

		public bool SaveUserToken;
		public bool SaveGuestToken;

		public enum SpaceJoinOption
		{
			JoinId = 0,
			SpacesList = 1,
			Custom = 2,
		}
		public SpaceJoinOption SpaceJoinMethod;

		public GameObject SpacesListMenu;

		public string AutomaticSpaceJoinId;

		public List<GameObject> LoadingMenus;

		public List<GameObject> SpaceMenus;

		public GameObject RemoteUserAvatar;
		public bool SpawnRemoteAvatars = true;
		public bool ShowLocalUser = true;
		
		[System.Serializable]
		public class CavrnusSpawnableObject
		{
			public string UniqueId;
			public GameObject Object;
		}

		public List<CavrnusSpawnableObject> SpawnableObjects;

        [System.Serializable]
        public class CavrnusSettings
		{
            public bool DisableVoice = false;
            public bool DisableVideo = false;
            public bool DisableAcousticEchoCancellation = false;
        }
        
		public CavrnusSettings AdditionalSettings;

        public static CavrnusSpatialConnector Instance => instance;
		private static CavrnusSpatialConnector instance;

		private List<GameObject> CurrentServerUi = new List<GameObject>();
		private void Start()
		{
			instance = this;

			//Parse and handle cmd line args
			foreach (var arg in System.Environment.GetCommandLineArgs())
			{
				if (arg.StartsWith("Server="))
				{
					MyServer = arg.Substring("Server=".Length);
				}
				if (arg.StartsWith("GuestName="))
				{
					AuthenticationMethod = AuthenticationOptionEnum.JoinAsGuest;
					GuestLoginMethod = GuestLoginOptionEnum.EnterNameBelow;
					GuestName = arg.Substring("GuestName=".Length);
				}
				if (arg.StartsWith("UserEmail="))
				{
					AuthenticationMethod = AuthenticationOptionEnum.JoinAsMember;
					MemberLoginMethod = MemberLoginOptionEnum.EnterMemberLoginCredentials;
					MemberEmail = arg.Substring("UserEmail=".Length);
				}
				if (arg.StartsWith("UserPassword="))
				{
					AuthenticationMethod = AuthenticationOptionEnum.JoinAsMember;
					MemberLoginMethod = MemberLoginOptionEnum.EnterMemberLoginCredentials;
					MemberPassword = arg.Substring("UserPassword=".Length);
				}
				if (arg.StartsWith("SpaceJoinId="))
				{
					SpaceJoinMethod = SpaceJoinOption.JoinId;
					AutomaticSpaceJoinId = arg.Substring("SpaceJoinId=".Length);
				}
			}

			if (!string.IsNullOrEmpty(MyServer))
			{
				Startup();
			}
			else
			{
				CurrentServerUi.Add(Instantiate(MyServerMenu, UiCanvas.transform));
			}
		}

		public void Startup()
		{
			foreach (var ui in CurrentServerUi)
				Destroy(ui);

			ValidateSpawnableObjects();

			DontDestroyOnLoad(this);

			CavrnusFunctionLibrary.InitializeCavrnus();

			SetupAuthenticate();

			if (SpawnRemoteAvatars)
				new CavrnusAvatarManager().Setup(RemoteUserAvatar, ShowLocalUser);
		}

		private HashSet<string> spawnedObjectUniqueIds = new HashSet<string>();
		private void ValidateSpawnableObjects()
		{
			foreach(var obj in SpawnableObjects)
			{
				if (obj.Object == null)
					Debug.LogError($"SpawnableObjects in the Cavrnus Spatial Connector is missing a prefab for ID \"{obj.UniqueId}\"");

				if (spawnedObjectUniqueIds.Contains(obj.UniqueId))
					Debug.LogError($"SpawnableObjects in the Cavrnus Spatial Connector already contains an object with ID \"{obj.UniqueId}\"");
				else
					spawnedObjectUniqueIds.Add(obj.UniqueId);
			}
		}

		private async Task<bool> IsTokenValid(string tokenName)
		{
			if (!string.IsNullOrWhiteSpace(PlayerPrefs.GetString(tokenName)))
			{
				var auth = await CavrnusAuthHelpers.TryAuthenticateWithToken(MyServer, PlayerPrefs.GetString(tokenName));

				return auth != null;
			}

			return false;
		}

		private List<GameObject> CurrentAuthenticationUi = new List<GameObject>();
		private async void SetupAuthenticate()
		{
			if (AuthenticationMethod != AuthenticationOptionEnum.Custom)
				CavrnusFunctionLibrary.AwaitAuthentication(auth => SetupJoinSpace());
			
			if (AuthenticationMethod == AuthenticationOptionEnum.JoinAsMember)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Server specified!");

				if (MemberLoginMethod == MemberLoginOptionEnum.EnterMemberLoginCredentials) {
					if (string.IsNullOrEmpty(MemberEmail))
						throw new System.Exception("Error on Cavrnus Spatial Connector object: No Username specified!");
					if (string.IsNullOrEmpty(MemberPassword))
						throw new System.Exception("Error on Cavrnus Spatial Connector object: No Password specified!");
					
					CavrnusFunctionLibrary.AuthenticateWithPassword(MyServer, MemberEmail, MemberPassword, auth => { }, err => Debug.LogError(err));
				} 
				else if (MemberLoginMethod == MemberLoginOptionEnum.PromptMemberToLogin) {
					if (SaveUserToken) {
						var valid = await IsTokenValid("MemberCavrnusAuthToken");
						if (valid) {
							return; // Escape early, if auth is successful then SetupJoinSpace() will be called from above await
						}
					}
					else {
						//Clear any existing token if Save is not set
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
					}
					
					if (MemberLoginMenu == null)
						throw new System.Exception("Error on Cavrnus Spatial Connector object: No Member Login Menu specified!");
					
					if (UiCanvas != null)
						CurrentAuthenticationUi.Add(Instantiate(MemberLoginMenu, UiCanvas.transform));
				}
			}
			else if (AuthenticationMethod == AuthenticationOptionEnum.JoinAsGuest)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Server specified!");

				if (GuestLoginMethod == GuestLoginOptionEnum.EnterNameBelow) {
					if (string.IsNullOrEmpty(GuestName))
						throw new System.Exception("Error on Cavrnus Spatial Connector object: No Guest Username specified!");
					
					CavrnusFunctionLibrary.AuthenticateAsGuest(MyServer, GuestName, auth => { }, err => Debug.LogError(err));
				} 
				else if (GuestLoginMethod == GuestLoginOptionEnum.PromptToEnterName) {
					if (SaveGuestToken) {
						var valid = await IsTokenValid("GuestCavrnusAuthToken");
						if (valid) {
							return; // Escape early, if auth is successful then SetupJoinSpace() will be called from above await
						}
					}
					else {
						//Clear any existing token if Save is not set
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");
					}
					
					if (GuestJoinMenu == null)
						throw new System.Exception("Error on Cavrnus Spatial Connector object: No Guest Join Menu specified!");
				
					if (UiCanvas != null)
						CurrentAuthenticationUi.Add(Instantiate(GuestJoinMenu, UiCanvas.transform));
				}
			}
		}
		
		private List<GameObject> CurrentSpaceJoinUi = new List<GameObject>();
		private void SetupJoinSpace()
		{
			foreach (var ui in CurrentAuthenticationUi)
				Destroy(ui);
			
			CurrentAuthenticationUi.Clear();

			if (SpaceJoinMethod == SpaceJoinOption.JoinId)
			{
				CavrnusFunctionLibrary.AwaitAnySpaceBeginLoading(spaceId => SetupLoadingUi(GetCurrentSpaceJoinUi(), false));
				
				if (string.IsNullOrEmpty(AutomaticSpaceJoinId))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Automatic Space Join ID specified!");

				CavrnusFunctionLibrary.JoinSpace(AutomaticSpaceJoinId, print, print);
			}
			else if (SpaceJoinMethod == SpaceJoinOption.SpacesList)
			{
				CavrnusFunctionLibrary.AwaitAnySpaceBeginLoading(spaceId => SetupLoadingUi(GetCurrentSpaceJoinUi(), true));

				if (SpacesListMenu == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Spaces List Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Canvas has been specified to contain the spawned UI!");

				CurrentSpaceJoinUi.Add(Instantiate(SpacesListMenu, UiCanvas.transform));
			}
		}

		private List<GameObject> CurrentSpaceLoadingUi = new List<GameObject>();

		private List<GameObject> GetCurrentSpaceJoinUi()
		{
			return CurrentSpaceJoinUi;
		}

		private void SetupLoadingUi(List<GameObject> currentSpaceJoinUi, bool required)
		{
			if (required && UiCanvas == null)
				throw new System.Exception("Error on Cavrnus Spatial Connector object: No Canvas has been specified to contain the spawned UI!");

			if (UiCanvas != null) {
				foreach (var ui in CurrentSpaceJoinUi)
					Destroy(ui);
				currentSpaceJoinUi.Clear();
				
					foreach (var ui in LoadingMenus)
						CurrentSpaceLoadingUi.Add(Instantiate(ui, UiCanvas.transform));
			}

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceId => FinalizeSpaceJoin(required));
		}
		
		private readonly List<GameObject> currentSpaceUi = new List<GameObject>();
		private void FinalizeSpaceJoin(bool required)
		{
			
			if (required && UiCanvas == null)
				throw new System.Exception("Error on Cavrnus Spatial Connector object: No Canvas has been specified to contain the spawned UI!");

			if (UiCanvas != null) {
				foreach (var ui in CurrentSpaceLoadingUi)
					Destroy(ui);
				CurrentSpaceLoadingUi.Clear();

				foreach (var ui in SpaceMenus)
					currentSpaceUi.Add(Instantiate(ui, UiCanvas.transform));
			}
		}
	}
}