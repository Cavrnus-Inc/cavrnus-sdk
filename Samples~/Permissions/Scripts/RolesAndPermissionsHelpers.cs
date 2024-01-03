using System;
using Collab.Proxy;
using Collab.Base.Collections;

namespace CavrnusSdk.Permissions
{
	public static class RoleAndPermissionHelpers
	{
		public static IDisposable EvaluateGlobalPolicy(string policy, Action<bool> onValueChanged)
		{
			var handle = Eval(policy, new Setting<PolicyContext>(new PolicyContext(GetLocalUserRoles())));
			handle.LiveValue.Bind(b => onValueChanged?.Invoke(b));
			
			return handle;
		}

		public static IDisposable EvaluateSpacePolicy(string policy, CavrnusSpaceConnection conn, Action<bool> onValueChanged)
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
			return CavrnusHelpers.LivePolicyEvaluator.EvaluatePolicy(policy, context);
		}

		private static RoleHash GetLocalUserRoles()
		{
			return CavrnusHelpers.Notify.ContextualRoles.GetRolesForContext(CavrnusHelpers.Notify.LocalUserId.Value);
		}
		
		private static RoleHash GetLocalRoomRoles(CavrnusSpaceConnection conn)
		{
			return CavrnusHelpers.Notify.ContextualRoles.GetRolesForContext(CavrnusHelpers.Notify.UsersSystem.ConnectedUser.Value.Id, conn.RoomSystem.Comm.SessionId);
		}

		private static bool RoomIsOwnedByLocalUser(CavrnusSpaceConnection conn)
		{
			var ownedByLocalUser = false;

			var roomId = conn.RoomSystem.Comm.SessionId;
			var userId = conn.RoomSystem.Comm.LocalCommUser.Value.User.Id;
			
			if (CavrnusHelpers.Notify.RoomsSystem.RoomsInfo.TryGetValue(roomId, out var value))
				ownedByLocalUser = value.OwnerId == userId;
			
			return ownedByLocalUser;
		}
	}
}