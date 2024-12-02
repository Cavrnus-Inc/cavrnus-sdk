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
		public string ConnectionId => connectionIdSetting?.Value;
		public string UserAccountId => userAccountIdSetting?.Value;
		
		public CavrnusSpaceConnection SpaceConnection { get; private set; }
		
		internal IReadonlySetting<string> ConnectionIdSetting => connectionIdSetting;
		private readonly ISetting<string> connectionIdSetting = new Setting<string>();

		internal IReadonlySetting<string> ContainerIdSetting => containerIdSetting;
		private readonly ISetting<string> containerIdSetting = new Setting<string>();
		
		private readonly ISetting<string> userAccountIdSetting = new Setting<string>();
		private readonly ISetting<UserVideoTextureProvider> vidProviderSetting = new Setting<UserVideoTextureProvider>();

		internal IReadonlySetting<UserVideoTextureProvider> VidProvider => vidProviderSetting;
		private ISessionCommunicationUser commUser;
		
		internal CavrnusUser(ISessionCommunicationUser commUser, CavrnusSpaceConnection spaceConn)
		{
			SpaceConnection = spaceConn;

			if (commUser is ISessionCommunicationLocalUser lu)
			{
				IsLocalUser = true;
				InitUser(lu);
				spaceConn.CurrentSpaceConnection.Hook(async sconn =>
				{
					try
					{
						var newlu = await sconn.RoomSystem.AwaitLocalUser();
						InitUser(newlu);
					}
					catch (Exception) {}
				});
			}
			else
			{
				IsLocalUser = false;
				InitUser(commUser);
			}
		}

		internal void InitUser(ISessionCommunicationUser cUser)
		{
			commUser = cUser;
			
			IsLocalUser = commUser is ISessionCommunicationLocalUser;
			userAccountIdSetting.Value = cUser.User.Id;
			connectionIdSetting.Value = cUser.ConnectionId;
			containerIdSetting.Value = $"/users/{cUser.ConnectionId}";
			vidProviderSetting.Value = new UserVideoTextureProvider(cUser);
		}
	}
}