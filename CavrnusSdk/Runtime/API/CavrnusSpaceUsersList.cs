using System;
using Collab.Base.Collections;
using Collab.LiveRoomSystem;
using Collab.Proxy.Comm;
using System.Collections.Generic;

namespace CavrnusSdk
{
	public class CavrnusSpaceUsersList
	{
		public ISetting<CavrnusUser> LocalUser = new Setting<CavrnusUser>();
		public NotifyList<CavrnusUser> Users = new NotifyList<CavrnusUser>();

		private RoomSystem rs;

		private List<IDisposable> disposables = new List<IDisposable>();

		private Dictionary<string, CavrnusUser> cavrnusUsersLookup =
			new Dictionary<string, CavrnusUser>();

		public CavrnusSpaceUsersList(RoomSystem rs)
		{
			this.rs = rs;

			var localUserBnd = rs.Comm.LocalCommUser.Bind(HandleLocalUser);
			disposables.Add(localUserBnd);

			var remoteUsersBnd = rs.Comm.ConnectedUsers.BindAll(RemoteUserAdded, LocalUserRemoved);
			disposables.Add(remoteUsersBnd);
		}

		private void RemoteUserAdded(ISessionCommunicationUser user)
		{
			var cu = new CavrnusUser(user, rs);
			cavrnusUsersLookup[user.ConnectionId] = cu;
			Users.Add(cu);
		}

		private void LocalUserRemoved(ISessionCommunicationUser user)
		{
			Users.Remove(cavrnusUsersLookup[user.ConnectionId]);
			cavrnusUsersLookup.Remove(user.ConnectionId);
		}

		private void HandleLocalUser(ISessionCommunicationLocalUser newLocalUser,
		                             ISessionCommunicationLocalUser oldLocalUser)
		{
			if (oldLocalUser != null) { LocalUserRemoved(oldLocalUser); }

			if (newLocalUser != null) {
				var cu = new CavrnusUser(newLocalUser, rs);
				LocalUser.Value = cu;
				cavrnusUsersLookup[newLocalUser.ConnectionId] = cu;
				Users.Insert(0, cu);
			}
		}

		public void Shutdown()
		{
			foreach (var disp in disposables) { disp.Dispose(); }
		}
	}
}