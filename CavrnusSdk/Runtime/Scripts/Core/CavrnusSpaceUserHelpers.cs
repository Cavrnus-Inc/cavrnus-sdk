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
			IDisposable lcuBind = null;
			IDisposable mapBind = null;
			NotifyListMapper<ISessionCommunicationRemoteUser, CavrnusUser> mapper = null;
			
			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				if (sc == null)
					return;
				
				if (lcu != null) {
					userRemoved?.Invoke(lcu);
					lcu = null;
				}
				
				lcuBind = sc.RoomSystem.Comm.LocalCommUser.Bind(lu => {
					if (lcu != null) {
						userRemoved?.Invoke(lcu);
						lcu = null;
					}
				
					if (lu != null) {
						lcu = new CavrnusUser(lu, spaceConn);
						userAdded?.Invoke(lcu);
					}
				});
				
				mapper?.Dispose();
				mapBind?.Dispose();
				
				mapper = new NotifyListMapper<ISessionCommunicationRemoteUser, CavrnusUser>(spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.ConnectedUsers);
				mapper.BeginMapping(ru => new CavrnusUser(ru, spaceConn));
				mapBind = mapper.Result.BindAll(userAdded, userRemoved);
			});
			
			return mapper.AlsoDispose(mapBind).AlsoDispose(spaceBind).AlsoDispose(lcuBind);
		}
	}
}