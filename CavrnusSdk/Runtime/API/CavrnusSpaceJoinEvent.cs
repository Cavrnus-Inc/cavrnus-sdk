using System;
using System.Collections.Generic;

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
			_currentLoadingSpace = null;

			_currentCavrnusSpace = cavrnusSpace;
			foreach (var joinAction in onSpaceJoinActions) joinAction?.Invoke(cavrnusSpace);
			onSpaceJoinActions.Clear();
		}

		public static void ExitCurrentSpace()
		{
			if (_currentCavrnusSpace != null) _currentCavrnusSpace.Dispose();

			_currentCavrnusSpace = null;
		}

		public static string CurrentLoadingSpace => _currentLoadingSpace;
		private static string _currentLoadingSpace;

		private static List<Action<string>> onSpaceLoadingActions =
			new List<Action<string>>();

		public static void OnSpaceLoading(Action<string> a)
		{
			if (_currentCavrnusSpace != null)
				a?.Invoke(_currentLoadingSpace);
			else
				onSpaceLoadingActions.Add(a);
		}

		public static void InvokeSpaceLoading(string space)
		{
			_currentLoadingSpace = space;
			foreach (var loadingAction in onSpaceLoadingActions) loadingAction?.Invoke(space);
			onSpaceLoadingActions.Clear();
		}
	}
}