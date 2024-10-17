using Collab.Base.Collections;
using Collab.Proxy.Comm;
using System;
using CavrnusCore;

namespace CavrnusSdk.API
{
	public class CavrnusUser
	{
		public bool IsLocalUser { get; private set; }

		public string ContainerId => containerIdSetting?.Value;
		public string UserAccountId => userAccountIdSetting?.Value;
		
		public CavrnusSpaceConnection SpaceConnection{ get; private set; }

		internal IReadonlySetting<string> ContainerIdSetting => containerIdSetting;
		private readonly ISetting<string> containerIdSetting = new Setting<string>();
		
		private readonly ISetting<string> userAccountIdSetting = new Setting<string>();
		private readonly ISetting<UserVideoTextureProvider> vidProviderSetting = new Setting<UserVideoTextureProvider>();

		internal UserVideoTextureProvider VidProvider => vidProviderSetting?.Value;
		private ISessionCommunicationUser commUser;
		
		internal CavrnusUser(ISessionCommunicationUser commUser, CavrnusSpaceConnection spaceConn)
		{
			SpaceConnection = spaceConn;

			InitUser(commUser);
		}

		internal void InitUser(ISessionCommunicationUser cUser)
		{
			commUser = cUser;
			
			IsLocalUser = commUser is ISessionCommunicationLocalUser;
			userAccountIdSetting.Value = cUser.User.Id;
			containerIdSetting.Value = $"/users/{cUser.ConnectionId}";
			vidProviderSetting.Value = new UserVideoTextureProvider(cUser);
		}
	}
}