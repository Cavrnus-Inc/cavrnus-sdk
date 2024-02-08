using Collab.Base.Collections;
using System;
using CavrnusSdk.API;

namespace CavrnusCore
{
	internal static class CavrnusSpaceUserHelpers
	{
		internal static IDisposable BindSpaceUsers(CavrnusSpaceConnection spaceConn, Action<CavrnusUser> userAdded, Action<CavrnusUser> userRemoved)
		{
			userAdded(new CavrnusUser(spaceConn.RoomSystem.Comm.LocalCommUser.Value, spaceConn));

            return spaceConn.RoomSystem.Comm.ConnectedUsers.BindAll(u => userAdded(new CavrnusUser(u, spaceConn)), u => userRemoved(new CavrnusUser(u, spaceConn)));
		}
	}

}
