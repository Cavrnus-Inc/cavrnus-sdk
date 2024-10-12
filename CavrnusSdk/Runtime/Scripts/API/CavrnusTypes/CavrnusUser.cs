using Collab.Base.Collections;
using Collab.Proxy.Comm;
using System;
using CavrnusCore;

namespace CavrnusSdk.API
{
	public class CavrnusUser : IDisposable
	{
		public bool IsLocalUser{ get; }

		public string ContainerId => containerIdSetting?.Value;
		public string UserAccountId => userAccountIdSetting?.Value;
		
		public CavrnusSpaceConnection SpaceConnection{ get; private set; }

		internal IReadonlySetting<string> ContainerIdSetting => containerIdSetting;
		private readonly ISetting<string> containerIdSetting = new Setting<string>();
		
		private readonly ISetting<string> userAccountIdSetting = new Setting<string>();
		private readonly ISetting<UserVideoTextureProvider> vidProviderSetting = new Setting<UserVideoTextureProvider>();

		internal UserVideoTextureProvider VidProvider => vidProviderSetting?.Value;
		private ISessionCommunicationUser commUser;
		
		private readonly IDisposable spaceBind;

		internal CavrnusUser(ISessionCommunicationUser commUser, CavrnusSpaceConnection spaceConn)
		{
			this.commUser = commUser;
			SpaceConnection = spaceConn;

			userAccountIdSetting.Value = commUser.User.Id;
			containerIdSetting.Value = $"/users/{commUser.ConnectionId}";
			vidProviderSetting.Value = new UserVideoTextureProvider(commUser);

			IsLocalUser = commUser is ISessionCommunicationLocalUser;

			if (IsLocalUser) {
				spaceBind = spaceConn.CurrentLocalUserSetting.Bind(lu => {
					if (lu == null)
						return;

					this.commUser = lu.commUser;
					userAccountIdSetting.Value = lu.commUser.User.Id;
					containerIdSetting.Value = $"/users/{lu.commUser.ConnectionId}";
					vidProviderSetting.Value = new UserVideoTextureProvider(lu.commUser);
				});
			}
		}

        public void Dispose()
        {
	        spaceBind?.Dispose();
        }
	}
}