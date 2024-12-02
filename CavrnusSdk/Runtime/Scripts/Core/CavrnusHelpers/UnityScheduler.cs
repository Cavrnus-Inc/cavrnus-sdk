using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Scheduler;
using UnityBase;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts
{
	[ExecuteAlways]
	public class UnityScheduler : MonoBehaviour, IUnityScheduler
	{
		private class ActionEntry
		{
			public Action action;
			public string taskName;
		}

		private class ActionEnumerationEntry
		{
			public Action singleAction;
			public Func<IEnumerable> action;
			public string taskName;
		}

		private List<ActionEntry> mainThreadActionQueue = new List<ActionEntry>();
		private Queue<ActionEnumerationEntry> mainThreadAsyncQueue = new Queue<ActionEnumerationEntry>();
		private List<ActionEntry> applicationQuitActionQueue = new List<ActionEntry>();

		private Thread mainUnityThread;

		public bool IsInMainThread() => Thread.CurrentThread == mainUnityThread;

		public Collab.Base.Scheduler.IScheduler BaseScheduler { get; private set; } = new Collab.Base.Scheduler.ManualOnlyScheduler();

		private bool asyncQueueIsRunning = false;

		private Stopwatch timingStopwatch;

		public void Setup()
		{
			mainUnityThread = Thread.CurrentThread;

			timingStopwatch = new Stopwatch();
			timingStopwatch.Reset();
			timingStopwatch.Start();
		}

		public void OnApplicationQuit()
		{
			ProcessActionQueue(applicationQuitActionQueue);
		}

		private List<List<ActionEntry>> actionsAfterFramesQueue = new List<List<ActionEntry>>()
		{
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
			new List<ActionEntry>(),
		};

		public void Update()
		{
			ProcessActionQueue(mainThreadActionQueue);

			double startq = timingStopwatch?.Elapsed.TotalSeconds ?? 0;
			int taskCt = 0;
			string first = null;

			//Process the actions that will happen on this frame
			var currentActions = actionsAfterFramesQueue[0];
			foreach (var action in currentActions)
			{
				double cur = timingStopwatch?.Elapsed.TotalSeconds ?? 0;
				action.action?.Invoke();
				first = first ?? action.taskName;
				taskCt++;
				if (((timingStopwatch?.Elapsed.TotalSeconds ?? 0) - cur) > .2)
					Debug.Log($"Task {action.taskName} over time, took {timingStopwatch?.Elapsed.TotalSeconds ?? 0f - cur:F3}s");
			}

			double endq = timingStopwatch?.Elapsed.TotalSeconds ?? 0;
			if (startq - endq > .2)
				Debug.Log($"Task frame over budget: Took {startq-endq:F4}s, {taskCt} unitytasks:\n{String.Join(",", currentActions.Select(a=>a.taskName))}");

			currentActions.Clear();
			actionsAfterFramesQueue.RemoveAt(0);
			actionsAfterFramesQueue.Add(currentActions);

			//DebugOutput.Info($"Sched Update time to {Time.time}.");

			(BaseScheduler as ManualOnlyScheduler)?.UpdateToTime(timingStopwatch?.Elapsed.TotalSeconds ?? 0f);


			if (!asyncQueueIsRunning)
			{
				bool anyAsync = false;
				lock (mainThreadAsyncQueue)
					anyAsync = mainThreadAsyncQueue.Any();

				if (anyAsync)
				{
					asyncQueueIsRunning = true;
					StartCoroutine(Helpers.WrapCoroutineWithMaxTime(ExecuteMainThreadAsyncQueue()));
				}
			}
		}

		private void ProcessActionQueue(List<ActionEntry> queue)
		{
			while (queue.Count > 0)
			{
				List<ActionEntry> copy;
				lock (queue)
				{
					copy = new List<ActionEntry>(queue);
					queue.Clear();
				}
				foreach (var a in copy)
				{
					try
					{
#if VERBOSESCHEDULERLOG
						if (a.taskName != null)
							DebugOutput.Log($"UnityScheduler Main Thread: \t\t\t\tBegin @ {timingStopwatch.Elapsed.TotalSeconds:F3}: {a.taskName}");
#endif
						a.action();
#if VERBOSESCHEDULERLOG
						if (a.taskName != null)
							DebugOutput.Log($"UnityScheduler Main Thread: \t\t\t\tEnd   @ {timingStopwatch.Elapsed.TotalSeconds:F3}: {a.taskName}");
#endif
					}
					catch (Exception e)
					{
						DebugOutput.Error($"Error from scheduled task '{a.taskName}': {e}");
					}
				}
			}
		}

		private IEnumerator ExecuteMainThreadAsyncQueue()
		{
			while (true)
			{
				ActionEnumerationEntry action = null;
				lock (mainThreadAsyncQueue)
				{
					if (mainThreadAsyncQueue.Any())
					{
						action = mainThreadAsyncQueue.Dequeue();
					}
				}
				if (action != null)
				{
#if VERBOSESCHEDULERLOG
					DebugOutput.Log($"UnityScheduler Main Async Coroutine Thread: Begin @ {timingStopwatch.Elapsed.TotalSeconds:F3}: {action.taskName}");
#endif
					if (action.singleAction != null)
					{
						action.singleAction();
						yield return null;
					}
					else if (action.action != null)
					{
						var enumerable = action.action();
						var enumerating = enumerable.GetEnumerator();
						while (true)
						{
							object stepYield;
							try
							{
								if (!enumerating.MoveNext())
									break;
								stepYield = enumerating.Current;
							}
							catch (Exception e)
							{
								DebugOutput.Error($"UnityScheduler Async Task Exception in task {action.taskName}: {e}");
								break;
							}
#if VERBOSESCHEDULERLOG
							DebugOutput.Log($"UnityScheduler Main Async Coroutine Thread: Yield @ {timingStopwatch.Elapsed.TotalSeconds:F3}: {action.taskName}");
#endif
							yield return stepYield;
						}
#if VERBOSESCHEDULERLOG
						DebugOutput.Log($"UnityScheduler Main Async Coroutine Thread: End   @ {timingStopwatch.Elapsed.TotalSeconds:F3}: {action.taskName}");
#endif
						yield return new WaitForSecondsRealtime(0.01f); // Allow a wait for a frame break between jobs
					}
				}
				else
				{
					yield return new WaitForSecondsRealtime(0.01f);
				}
			}
		}
		
		public void ExecInMainThread(Action a, [CallerMemberName] string taskName = "")
		{
			lock (mainThreadActionQueue)
				mainThreadActionQueue.Add(new ActionEntry() { action = a, taskName = taskName });
		}
		
		public void ExecInMainThreadAsync(Action a, [CallerMemberName] string taskName = "")
		{
			lock (mainThreadAsyncQueue)
				mainThreadAsyncQueue.Enqueue(new ActionEnumerationEntry() { singleAction = a, taskName = taskName });
		}

		public void ExecInMainThreadAsync(Func<IEnumerable> a, [CallerMemberName] string taskName = "")
		{
			lock (mainThreadAsyncQueue)
				mainThreadAsyncQueue.Enqueue(new ActionEnumerationEntry() { action = a, taskName = taskName });
		}
		public void ExecInMainThreadOrImmediate(Action a, string taskName = "")
		{
			if (IsInMainThread())
				a();
			else 
				ExecInMainThread(a, taskName);
		}

		public void ExecOnApplicationQuit(Action a, string taskName = "")
		{
			lock (applicationQuitActionQueue)
				applicationQuitActionQueue.Add(new ActionEntry() { action = a, taskName = taskName });
		}

		public Coroutine ExecCoRoutine(IEnumerator coRoutine)
		{
			return StartCoroutine(coRoutine);
		}
		public void CancelCoRoutine(Coroutine coRoutine)
		{
			if (HelperFunctions.NullOrDestroyed(this))
				return;
			StopCoroutine(coRoutine);
		}
		
		public void ExecInMainThreadAfterDelay(float delay, Action action, [CallerMemberName] string taskName = "")
		{
			BaseScheduler.InsertTask(delay, action, taskName);
		}

		public void ExecInMainThreadAfterFrames(int frames, Action action, string taskName = "")
		{
			if (frames == 0)
			{
				return;
			}
			if (frames >= actionsAfterFramesQueue.Count)
				frames = actionsAfterFramesQueue.Count - 1;
			actionsAfterFramesQueue[frames].Add(new ActionEntry() { action = action, taskName = taskName });
		}
	}
}