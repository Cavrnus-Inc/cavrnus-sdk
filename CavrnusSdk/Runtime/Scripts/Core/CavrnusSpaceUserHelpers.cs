using Collab.Base.Collections;
using System;
using CavrnusSdk.API;
using Collab.Base.Core;
using Collab.Proxy.Comm;

namespace CavrnusCore
{
	internal static class CavrnusSpaceUserHelpers
	{
		internal static IDisposable BindSpaceUsers(CavrnusSpaceConnection spaceConn, Action<CavrnusUser> userAdded, Action<CavrnusUser> userRemoved)
		{
			CavrnusUser lcu = null;
			var lcuBind = spaceConn.RoomSystem.Comm.LocalCommUser.Bind(lu => {
				if (lcu != null) {
					userRemoved?.Invoke(lcu);
					lcu = null;
				}

				if (lu != null) {
					lcu = new CavrnusUser(lu, spaceConn);
					userAdded?.Invoke(lcu);
				}
			});
			
			var mapper = new NotifyListMapper<ISessionCommunicationRemoteUser, CavrnusUser>(spaceConn.RoomSystem.Comm.ConnectedUsers);
			mapper.BeginMapping(ru => new CavrnusUser(ru, spaceConn));
			var mapBind = mapper.Result.BindAll(userAdded, userRemoved);
			
			return lcuBind.AlsoDispose(mapBind).AlsoDispose(mapper);
		}
	}
}