using Collab.Base.Core;
using Collab.Proxy.Comm.RestApi;
using Collab.Proxy.Comm;
using System.Collections.Generic;
using System;
using CavrnusSdk.API;

namespace CavrnusCore
{
    internal static class CavrnusAuthHelpers
    {
		internal static CavrnusAuthentication CurrentAuthentication = null;

		internal static async void Authenticate(string server, string email, string password, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.LoginRequest req = new RestUserCommunication.LoginRequest();
			req.email = email;
			req.password = password;
			var token = await ruc.PostLocalAccountLoginAsync(req);

			DebugOutput.Info("Logged in as User, token: " + token.token);

			CurrentAuthentication = new CavrnusAuthentication(endpoint.WithAuthorization(token.token));

			NotifySetup();

			HandleAuth(CurrentAuthentication);
			onSuccess(CurrentAuthentication);
		}

		internal static async void AuthenticateAsGuest(string server, string userName, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			RestApiEndpoint endpoint = RestApiEndpoint.ParseFromHostname(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.GuestRegistrationRequest req = new RestUserCommunication.GuestRegistrationRequest();
			req.screenName = userName;
			req.expires = DateTime.UtcNow.AddDays(7);

			var token = await ruc.PostGuestRegistrationAsync(req);

			DebugOutput.Info("Logged in as Guest, token: " + token.token);

			CurrentAuthentication = new CavrnusAuthentication(endpoint.WithAuthorization(token.token));

			NotifySetup();

			HandleAuth(CurrentAuthentication);
			onSuccess(CurrentAuthentication);
		}

		private static void NotifySetup()
		{
			CavrnusStatics.Notify.Initialize(CurrentAuthentication.Endpoint, true);
			CavrnusStatics.Notify.ObjectsSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.PoliciesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.RolesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.UsersSystem.StartListening(null, err => DebugOutput.Error(err.ToString()));
		}

		private static void HandleAuth(CavrnusAuthentication auth)
		{

			if(onAuthActions.Count > 0)
			{
				foreach(var action in onAuthActions)
				{
					action?.Invoke(auth);
				}

				onAuthActions.Clear();
			}
		}

		private static List<Action<CavrnusAuthentication>> onAuthActions = new List<Action<CavrnusAuthentication>>();

		internal static void AwaitAuthentication(Action<CavrnusAuthentication> onAuth)
		{
			if(CurrentAuthentication != null)
			{
				onAuth(CurrentAuthentication);
			}
			else
			{
				onAuthActions.Add(onAuth);
			}
		}
	}
}