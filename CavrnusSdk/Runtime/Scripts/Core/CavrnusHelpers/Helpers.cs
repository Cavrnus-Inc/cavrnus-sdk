using System;
using System.Collections;
using Collab.Base.Core;
using UnityEngine;

namespace UnityBase
{
	public static class Helpers
	{
		public static IEnumerator WrapCoroutineWithMaxTime(IEnumerator coRoutine)
		{
			return WrapCoroutineWithMaxTime(coRoutine, () => { });
		}

		public class TimingInfo
		{
			public enum Difficulty
			{
				Hard,
				OK
			}

			public Difficulty difficulty;

			public TimingInfo(Difficulty d)
			{
				difficulty = d;
			}
		}

		public static int MaxTimeCoroutineMaxMilliseconds = 4;

		public static IEnumerator WrapCoroutineWithMaxTime(IEnumerator coRoutine, Action onComplete)
		{
			long currMs = DateTimeCache.MsSinceStart;

			while (coRoutine.MoveNext())
			{
				var cur = coRoutine.Current;

				if (cur != null && (cur is CustomYieldInstruction || cur is YieldInstruction))
				{
					yield return cur;
					currMs = DateTimeCache.MsSinceStart;
					continue;
				}

				bool force = cur is TimingInfo &&
							(cur as TimingInfo).difficulty == TimingInfo.Difficulty.Hard;

				var nextms = DateTimeCache.MsSinceStart;
				var timepassed = nextms - currMs;

				//Debug.Log($"Async actions: from time {currMs}, is {nextms}, passing {timepassed}.");

				if (force || timepassed >= MaxTimeCoroutineMaxMilliseconds)
				{
					yield return new WaitForSecondsRealtime(.015f);
					currMs = DateTimeCache.MsSinceStart;
				}
			}
			onComplete();
		}

		public static void SetLayerRecursive(this GameObject obj, int layer)
		{
			foreach (var componentsInChild in obj.transform.GetComponentsInChildren<Transform>(true))
			{
				componentsInChild.gameObject.layer = layer;
			}
		}
	}
}