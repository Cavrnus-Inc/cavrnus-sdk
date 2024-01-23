using Collab.Proxy.Comm.RestApi;
using System.Collections.Generic;
using System;

namespace CavrnusSdk
{
	public class CavrnusAuth
	{
		public RestApiEndpoint Endpoint;

		public CavrnusAuth(RestApiEndpoint endpoint)
		{
			Endpoint = endpoint;
		}
	}

	public static class CavrnusAuthRecvEvent
	{
		public static CavrnusAuth CurrentAuth => _currentAuth;
		private static CavrnusAuth _currentAuth;

		private static List<Action<CavrnusAuth>> onAuthActions =
			new List<Action<CavrnusAuth>>();

		public static void OnAuthorization(Action<CavrnusAuth> a)
		{
			if (_currentAuth != null)
				a?.Invoke(_currentAuth);
			else
				onAuthActions.Add(a);
		}

		public static void SetAuthorization(CavrnusAuth auth)
		{
			_currentAuth = auth;
			foreach (var joinAction in onAuthActions) joinAction?.Invoke(auth);
			onAuthActions.Clear();
		}

		public static void Logout()
		{
			_currentAuth = null;
		}
	}
}