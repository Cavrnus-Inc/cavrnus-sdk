using System.Collections.Generic;
using CavrnusCore;
using UnityEngine;
using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;

namespace CavrnusSdk.Setup
{
	public class CavrnusSpatialConnector : MonoBehaviour
	{
		public string MyServer;

		public Canvas UiCanvas;

		public enum LoginOption
		{
			GuestJoinAutomatic = 0,
			GuestJoinManual = 1,
			LoginAutomatic = 2,
			LoginManual = 3,
			None = 4,
		}

		public LoginOption AuthenticationMethod;

		public GameObject ManualLoginMenu;

		public string AutomaticLoginEmail;
		public string AutomaticLoginPassword;

		public GameObject ManualGuestJoinMenu;

		public string AutomaticGuestJoinUsername;

		public bool SaveUserToken;

		public enum SpaceJoinOption
		{
			Automatic = 0,
			SpacesList = 1,
			None = 2,
		}
		public SpaceJoinOption SpaceJoinMethod;

		public GameObject SpacesListMenu;

		public string AutomaticSpaceJoinId;

		public List<GameObject> LoadingMenus;

		public List<GameObject> SpaceMenus;

		public GameObject RemoteUserAvatar;
		
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
            [Header("Some devices don't support voice and video systems. " + "Disabling this will prevent build/runtime errors on them.")]
            public bool DisableVoiceAndVideo = false;
            [Header("Some devices don't support AEC. " + "Disabling this will prevent build/runtime errors on them.")]
            public bool DisableAcousticEchoCancellation = false;
        }
        
		public CavrnusSettings AdditionalSettings;

        public static CavrnusSpatialConnector Instance => instance;
		private static CavrnusSpatialConnector instance;
		void Start()
		{
			ValidateSpawnableObjects();

            instance = this;

			GameObject.DontDestroyOnLoad(this);

			CavrnusFunctionLibrary.InitializeCavrnus();

			SetupAuthenticate();			
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

		private List<GameObject> CurrentAuthenticationUi = new List<GameObject>();
		private void SetupAuthenticate()
		{
			if (AuthenticationMethod != LoginOption.None)
				CavrnusFunctionLibrary.AwaitAuthentication(auth => SetupJoinSpace());

			if (AuthenticationMethod == LoginOption.GuestJoinAutomatic)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Server specified!");
				if (string.IsNullOrEmpty(AutomaticGuestJoinUsername))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Automatic Guest Join Username specified!");

				CavrnusFunctionLibrary.AuthenticateAsGuest(MyServer, AutomaticGuestJoinUsername, auth => { }, err => Debug.LogError(err));
			}
			else if (AuthenticationMethod == LoginOption.GuestJoinManual)
			{
				if (ManualGuestJoinMenu == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Manual Guest Join Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Canvas has been specified to contain the spawned UI!");

				CurrentAuthenticationUi.Add(GameObject.Instantiate(ManualGuestJoinMenu, UiCanvas.transform));
			}
			else if (AuthenticationMethod == LoginOption.LoginAutomatic)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Server specified!");
				if (string.IsNullOrEmpty(AutomaticLoginEmail))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Automatic Guest Join Username specified!");
				if (string.IsNullOrEmpty(AutomaticLoginPassword))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Automatic Guest Join Username specified!");

				CavrnusFunctionLibrary.AuthenticateWithPassword(MyServer, AutomaticLoginEmail, AutomaticLoginPassword, auth => { }, err => Debug.LogError(err));
			}
			else if (AuthenticationMethod == LoginOption.LoginManual)
			{
				if (ManualLoginMenu == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Manual Login Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Canvas has been specified to contain the spawned UI!");

				CurrentAuthenticationUi.Add(GameObject.Instantiate(ManualLoginMenu, UiCanvas.transform));
			}			
		}

		private List<GameObject> CurrentSpaceJoinUi = new List<GameObject>();
		private void SetupJoinSpace()
		{
			foreach (var ui in CurrentAuthenticationUi)
				GameObject.Destroy(ui);
			CurrentAuthenticationUi.Clear();

			if (SpaceJoinMethod == SpaceJoinOption.Automatic)
			{
				CavrnusFunctionLibrary.AwaitAnySpaceBeginLoading(spaceId => SetupLoadingUi(GetCurrentSpaceJoinUi(), false));
				
				if (string.IsNullOrEmpty(AutomaticSpaceJoinId))
					throw new System.Exception("Error on Cavrnus Spatial Connector object: No Automatic Space Join ID specified!");

				CavrnusFunctionLibrary.JoinSpace(AutomaticSpaceJoinId, spaceConn => { }, err => Debug.LogError(err));
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

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceId => FinalizeSpaceJoin(spaceId, required));
		}

		private readonly List<GameObject> currentSpaceUi = new List<GameObject>();
		private void FinalizeSpaceJoin(CavrnusSpaceConnection conn, bool required)
		{
			if (RemoteUserAvatar != null)
				conn.BindSpaceUsers(UserAdded, UserRemoved);
			
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

		private Dictionary<string, GameObject> avatarInstances = new Dictionary<string, GameObject>();

		//Instantiate avatars when we get a new user
		private void UserAdded(CavrnusUser user)
		{
			//This list contains the player. But we don't wanna show their avatar via this system.
			if (user.IsLocalUser)
				return;

			var avatar = Instantiate(RemoteUserAvatar, transform);
			avatar.AddComponent<CavrnusUserFlag>().User = user;

			CavrnusPropertyHelpers.ResetLiveHierarchyRootName(avatar, $"{user.ContainerId}");

			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
				sync.SendMyChanges = false;
			foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
				sync.SendMyChanges = false;

			avatarInstances[user.ContainerId] = avatar;
		}

		//Destroy them when we lose that user
		private void UserRemoved(CavrnusUser user)
		{
			if (avatarInstances.ContainsKey(user.ContainerId))
			{
				Destroy(avatarInstances[user.ContainerId].gameObject);
				avatarInstances.Remove(user.ContainerId);
			}
		}
	}
}