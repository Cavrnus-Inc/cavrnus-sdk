using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusCore : MonoBehaviour
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

		public CavrnusSpawnablePrefabsLookup SpawnablePrefabs;
		public CavrnusSettings CavrnusSettings;

		public static CavrnusCore Instance => instance;
		private static CavrnusCore instance;
		void Start()
		{
			instance = this;

			GameObject.DontDestroyOnLoad(this);

			CavrnusHelpers.Setup();

			SetupAuthenticate();			
		}

		private List<GameObject> CurrentAuthenticationUi = new List<GameObject>();
		private async void SetupAuthenticate()
		{
			if (AuthenticationMethod != LoginOption.None)
				CavrnusAuthRecvEvent.OnAuthorization(auth => SetupJoinSpace());

			if (AuthenticationMethod == LoginOption.GuestJoinAutomatic)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Core object: No Server specified!");
				if (string.IsNullOrEmpty(AutomaticGuestJoinUsername))
					throw new System.Exception("Error on Cavrnus Core object: No Automatic Guest Join Username specified!");

				await CavrnusHelpers.AuthenticateAsGuest(MyServer, AutomaticGuestJoinUsername);
			}
			else if (AuthenticationMethod == LoginOption.GuestJoinManual)
			{
				if (ManualGuestJoinMenu == null)
					throw new System.Exception("Error on Cavrnus Core object: No Manual Guest Join Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Core object: No Canvas has been specified to contain the spawned UI!");

				CurrentAuthenticationUi.Add(GameObject.Instantiate(ManualGuestJoinMenu, UiCanvas.transform));
			}
			else if (AuthenticationMethod == LoginOption.LoginAutomatic)
			{
				if (string.IsNullOrEmpty(MyServer))
					throw new System.Exception("Error on Cavrnus Core object: No Server specified!");
				if (string.IsNullOrEmpty(AutomaticLoginEmail))
					throw new System.Exception("Error on Cavrnus Core object: No Automatic Guest Join Username specified!");
				if (string.IsNullOrEmpty(AutomaticLoginPassword))
					throw new System.Exception("Error on Cavrnus Core object: No Automatic Guest Join Username specified!");

				await CavrnusHelpers.Authenticate(MyServer, AutomaticLoginEmail, AutomaticLoginPassword);
			}
			else if (AuthenticationMethod == LoginOption.LoginManual)
			{
				if (ManualLoginMenu == null)
					throw new System.Exception("Error on Cavrnus Core object: No Manual Login Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Core object: No Canvas has been specified to contain the spawned UI!");

				CurrentAuthenticationUi.Add(GameObject.Instantiate(ManualLoginMenu, UiCanvas.transform));
			}			
		}

		private List<GameObject> CurrentSpaceJoinUi = new List<GameObject>();
		private async void SetupJoinSpace()
		{
			foreach (var ui in CurrentAuthenticationUi)
				GameObject.Destroy(ui);
			CurrentAuthenticationUi.Clear();

			if (SpaceJoinMethod != SpaceJoinOption.None)
				CavrnusSpaceJoinEvent.OnSpaceLoading(spaceConn => SetupLoadingUi(GetCurrentSpaceJoinUi()));

			if (SpaceJoinMethod == SpaceJoinOption.Automatic)
			{
				if (string.IsNullOrEmpty(AutomaticSpaceJoinId))
					throw new System.Exception("Error on Cavrnus Core object: No Automatic Space Join ID specified!");

				await CavrnusHelpers.JoinSpaceFromId(AutomaticSpaceJoinId);
			}
			else if (SpaceJoinMethod == SpaceJoinOption.SpacesList)
			{
				if (SpacesListMenu == null)
					throw new System.Exception("Error on Cavrnus Core object: No Spaces List Menu specified!");
				if (UiCanvas == null)
					throw new System.Exception("Error on Cavrnus Core object: No Canvas has been specified to contain the spawned UI!");

				CurrentSpaceJoinUi.Add(GameObject.Instantiate(SpacesListMenu, UiCanvas.transform));
			}
		}

		private List<GameObject> CurrentSpaceLoadingUi = new List<GameObject>();

		private List<GameObject> GetCurrentSpaceJoinUi()
		{
			return CurrentSpaceJoinUi;
		}

		private void SetupLoadingUi(List<GameObject> currentSpaceJoinUi)
		{
			if (UiCanvas == null)
				throw new System.Exception("Error on Cavrnus Core object: No Canvas has been specified to contain the spawned UI!");

			foreach (var ui in CurrentSpaceJoinUi)
				GameObject.Destroy(ui);
			currentSpaceJoinUi.Clear();

			foreach (var ui in LoadingMenus)
				CurrentSpaceLoadingUi.Add(GameObject.Instantiate(ui, UiCanvas.transform));

			CavrnusSpaceJoinEvent.OnAnySpaceConnection(spaceConn => FinalizeSpaceJoin());
		}

		private List<GameObject> CurrentSpaceUi = new List<GameObject>();
		private void FinalizeSpaceJoin()
		{
			if (UiCanvas == null)
				throw new System.Exception("Error on Cavrnus Core object: No Canvas has been specified to contain the spawned UI!");

			foreach (var ui in CurrentSpaceLoadingUi)
				GameObject.Destroy(ui);
			CurrentSpaceLoadingUi.Clear();

			foreach (var ui in SpaceMenus)
				CurrentSpaceUi.Add(GameObject.Instantiate(ui, UiCanvas.transform));
		}
	}
}