using Collab.Base.Core;
using Collab.Proxy.Comm.RestApi;
using Collab.Proxy.Comm;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using CavrnusSdk.API;
using UnityEngine;
using System.Threading.Tasks;
using CavrnusSdk.Setup;
using Collab.Proxy.Comm.NotifyApi;
using Collab.Base.Collections;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("Tests")]

namespace CavrnusCore
{
    internal static class CavrnusAuthHelpers
    {
		internal static async Task<CavrnusAuthentication> TryAuthenticateWithToken(string server, string token)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server).WithAuthorization(token);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			
			try
			{
				await ruc.GetUserProfileAsync();
			}
			catch (ErrorInfo e)
			{
				if (e.status == 401)
				{
					//Invalid Token
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
			
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");

					return null;
				}

				//Fail, but don't clear the token
				//TODO: Is this right?
				return null;
			}

			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(ruc, endpoint, token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate((INotifyDataUser lu) => lu != null));

			HandleAuth(CavrnusStatics.CurrentAuthentication);
			return CavrnusStatics.CurrentAuthentication;
		}

		internal static async void Authenticate(string server, string email, string password, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.LoginRequest req = new RestUserCommunication.LoginRequest();
			req.email = email;
			req.password = password;

			TokenResult token = null;
			try
			{
				token = await ruc.PostLocalAccountLoginAsync(req);
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");

					Debug.LogError($"Invalid Server: {server}");
				}
				else
				{
					Debug.LogError(e.ToString());
				}
				return;
			}
			catch (ErrorInfo clientError)
			{
				if (clientError.status == 401)
				{
					//Invalid Token
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");
				}
				Debug.LogError(clientError.ToString());
				return;
			}

			DebugOutput.Info("Logged in as User, token: " + token.token);

			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(ruc, endpoint.WithAuthorization(token.token), token.token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate((INotifyDataUser lu) => lu != null));

			HandleAuth(CavrnusStatics.CurrentAuthentication);
			onSuccess(CavrnusStatics.CurrentAuthentication);
		}

		internal static async void AuthenticateAsGuest(string server, string userName, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.GuestRegistrationRequest req = new RestUserCommunication.GuestRegistrationRequest();
			req.screenName = userName;
			req.expires = DateTime.UtcNow.AddDays(7);

			string token = "";
			try
			{
				var tokenRes = await ruc.PostGuestRegistrationAsync(req);
				token = tokenRes.token;
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					Debug.LogError($"Invalid Server: {server}");
				}
				else
				{
					Debug.LogError(e.ToString());
				}
				return;
			}
			catch (ErrorInfo clientError)
			{
				Debug.LogError(clientError.ToString());
				return;
			}


			DebugOutput.Info("Logged in as Guest, token: " + token);

			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(ruc, endpoint.WithAuthorization(token), token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate(lu => {
				if (lu != null) {
					
					return true;
				}

				return false;
			}));

			HandleAuth(CavrnusStatics.CurrentAuthentication);
			onSuccess(CavrnusStatics.CurrentAuthentication);
		}

		private static void NotifySetup()
		{
			CavrnusStatics.Notify.Initialize(CavrnusStatics.CurrentAuthentication.Endpoint, true);
			CavrnusStatics.Notify.ObjectsSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.PoliciesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.RolesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.UsersSystem.StartListening(null, err => DebugOutput.Error(err.ToString()));

			CavrnusStatics.ContentManager.SetEndpoint(CavrnusStatics.CurrentAuthentication.Endpoint);
		}

		private static void HandleAuth(CavrnusAuthentication auth)
		{
			NotifySetup();
			
			if (CavrnusSpatialConnector.Instance.SaveUserToken)
			{
				PlayerPrefs.SetString("MemberCavrnusAuthToken", auth.Token);
			}
			
			if (CavrnusSpatialConnector.Instance.SaveGuestToken)
			{
				PlayerPrefs.SetString("GuestCavrnusAuthToken", auth.Token);
			}

			if(OnAuthActions.Count > 0)
			{
				foreach(var action in OnAuthActions)
				{
					action?.Invoke(auth);
				}

				OnAuthActions.Clear();
			}
		}

		private static readonly List<Action<CavrnusAuthentication>> OnAuthActions = new List<Action<CavrnusAuthentication>>();

		internal static void AwaitAuthentication(Action<CavrnusAuthentication> onAuth)
		{
			if(CavrnusStatics.CurrentAuthentication != null)
			{
				onAuth(CavrnusStatics.CurrentAuthentication);
			}
			else
			{
				OnAuthActions.Add(onAuth);
			}
		}
	}
}