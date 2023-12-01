using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public static class CavrnusSpaceJoinEvent
	{
		public static CavrnusSpaceConnection CurrentCavrnusSpace => _currentCavrnusSpace;
		private static CavrnusSpaceConnection _currentCavrnusSpace;

		private static List<Action<CavrnusSpaceConnection>> onSpaceJoinActions =
			new List<Action<CavrnusSpaceConnection>>();

		public static void OnAnySpaceConnection(Action<CavrnusSpaceConnection> a)
		{
			if (_currentCavrnusSpace != null)
				a?.Invoke(_currentCavrnusSpace);
			else
				onSpaceJoinActions.Add(a);
		}

		public static void InvokeSpaceJoin(CavrnusSpaceConnection cavrnusSpace)
		{
			_currentCavrnusSpace = cavrnusSpace;
			foreach (var joinAction in onSpaceJoinActions) joinAction?.Invoke(cavrnusSpace);
			onSpaceJoinActions.Clear();
		}

		public static void ExitCurrentSpace()
		{
			if (_currentCavrnusSpace != null) _currentCavrnusSpace.Dispose();

			_currentCavrnusSpace = null;
		}
	}
}