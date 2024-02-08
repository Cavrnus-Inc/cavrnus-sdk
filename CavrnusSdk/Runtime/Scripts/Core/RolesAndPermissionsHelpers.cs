using System;
using Collab.Proxy;
using Collab.Base.Collections;
using CavrnusSdk.API;

namespace CavrnusCore
{
	internal static class RoleAndPermissionHelpers
	{
		internal static IDisposable EvaluateGlobalPolicy(string policy, Action<bool> onValueChanged)
		{
			var handle = Eval(policy, new Setting<PolicyContext>(new PolicyContext(GetLocalUserRoles())));
			handle.LiveValue.Bind(b => onValueChanged?.Invoke(b));
			
			return handle;
		}

		internal static IDisposable EvaluateSpacePolicy(string policy, CavrnusSpaceConnection conn, Action<bool> onValueChanged)
		{
			var context = new PolicyContext(GetLocalUserRoles(), GetLocalRoomRoles(conn))
			{
				SpaceContext = new PolicyEvalSpaceContext
				{
					OwnedByLocalUser = RoomIsOwnedByLocalUser(conn),
				}
			};

			var handle = Eval(policy, new Setting<PolicyContext>(context));
			handle.LiveValue.Bind(b => onValueChanged?.Invoke(b));
			
			return handle;
		}

		private static IEvaluatedPolicyHandle Eval(string policy, IReadonlySetting<PolicyContext> context)
		{
			return CavrnusStatics.LivePolicyEvaluator.EvaluatePolicy(policy, context);
		}

		private static RoleHash GetLocalUserRoles()
		{
			return CavrnusStatics.Notify.ContextualRoles.GetRolesForContext(CavrnusStatics.Notify.LocalUserId.Value);
		}
		
		private static RoleHash GetLocalRoomRoles(CavrnusSpaceConnection conn)
		{
			return CavrnusStatics.Notify.ContextualRoles.GetRolesForContext(CavrnusStatics.Notify.UsersSystem.ConnectedUser.Value.Id, conn.RoomSystem.Comm.SessionId);
		}

		private static bool RoomIsOwnedByLocalUser(CavrnusSpaceConnection conn)
		{
			var ownedByLocalUser = false;

			var roomId = conn.RoomSystem.Comm.SessionId;
			var userId = conn.RoomSystem.Comm.LocalCommUser.Value.User.Id;
			
			if (CavrnusStatics.Notify.RoomsSystem.RoomsInfo.TryGetValue(roomId, out var value))
				ownedByLocalUser = value.OwnerId == userId;
			
			return ownedByLocalUser;
		}
	}
}