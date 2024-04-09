using Collab.Base.Collections;
using Collab.Proxy.Comm;
using Collab.Proxy.Comm.LocalTypes;
using System;
using UnityBase;
using CavrnusCore;

namespace CavrnusSdk.API
{
	public class CavrnusUser : IDisposable
	{
		public bool IsLocalUser;

		public string ContainerId;

		public CavrnusSpaceConnection SpaceConnection;

		internal UserVideoTextureProvider vidProvider;

        private ISessionCommunicationUser user;
		private IRegistrationHook<TextureWithUVs, int> userVidHook;

		internal CavrnusUser(ISessionCommunicationUser user, CavrnusSpaceConnection spaceConn)
		{
			this.user = user;
			SpaceConnection = spaceConn;

			ContainerId = $"/users/{user.ConnectionId}";

			IsLocalUser = user is ISessionCommunicationLocalUser;

			vidProvider = new UserVideoTextureProvider(user);
		}

		internal IDisposable BindLatestCoPresence(Action<CoPresenceLive> act)
		{
			return user.LatestCoPresence.Bind(act);
		}

        public void Dispose()
        {
	        userVidHook.Release();
        }
	}
}