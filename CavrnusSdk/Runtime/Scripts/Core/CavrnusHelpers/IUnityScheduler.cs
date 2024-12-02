using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Base.Scheduler;
using UnityEngine;

namespace UnityBase
{
	public interface IUnityScheduler
	{
		Collab.Base.Scheduler.IScheduler BaseScheduler { get; }

		void Setup();

		void ExecInMainThread(Action a, [CallerMemberName]string taskName = "");

		void ExecInMainThreadAsync(Action a, [CallerMemberName] string taskName = "");
		void ExecInMainThreadAsync(Func<IEnumerable> a, [CallerMemberName]string taskName = "");

		void ExecInMainThreadOrImmediate(Action a, [CallerMemberName] string taskName = "");

		void ExecInMainThreadAfterDelay(float delay, Action action, [CallerMemberName]string taskName = "");

		void ExecInMainThreadAfterFrames(int frames, Action action, [CallerMemberName] string taskName = "");

		void ExecOnApplicationQuit(Action action, [CallerMemberName] string taskName = "");

		Coroutine ExecCoRoutine(IEnumerator coRoutine);
		void CancelCoRoutine(Coroutine coRoutine);

		bool IsInMainThread();
	}


	public static class UnitySchedulerExtensions
	{
		public static Task ExecInMainThreadTask(this IUnityScheduler sched, Action a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteTask(a, tcs), taskName);
			return tcs.Task;
		}
		public static Task ExecInMainThreadTask(this IUnityScheduler sched, Func<Task> a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			sched.ExecInMainThreadAsync(async () =>
			{
				try
				{
					await a();
					tcs.SetResult(true);
				}
				catch (Exception e)
				{
					DebugOutput.DError($"Exception in MainThreadTask: {e}");
					tcs.SetException(e);
				}
			}, taskName);
			return tcs.Task;
		}		
		public static Task<T> ExecInMainThreadTask<T>(this IUnityScheduler sched, Func<T> a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteFuncTask(a, tcs), taskName);
			return tcs.Task;
		}
		public static Task<T> ExecInMainThreadAsAsyncTask<T>(this IUnityScheduler sched, Func<Task<T>> asyncAct, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteAsyncFuncTask(asyncAct, tcs), taskName);
			return tcs.Task;
		}

		public static Task ExecInMainThreadAsyncTask(this IUnityScheduler sched, Action a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteTask(a, tcs), taskName);
			return tcs.Task;
		}
		public static Task<T> ExecInMainThreadAsyncTask<T>(this IUnityScheduler sched, Func<T> a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteFuncTask(a, tcs), taskName);
			return tcs.Task;
		}

		public static Task<T> ExecInMainThreadTask<T>(this IUnityScheduler sched, Func<Task<T>> a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			sched.ExecInMainThreadAsync(async () =>
			{
				try
				{
					var r = await a();
					tcs.SetResult(r);
				}
				catch (Exception e)
				{
					// The exception is rethrown, catch there since sometimes this can happen in un-exceptional cases.
					//DebugOutput.DLog($"Exception in MainThreadTask: {e}");
					tcs.SetException(e);
				}
			}, taskName);
			return tcs.Task;
		}

		private static void ExecThenCompleteTask(Action act, TaskCompletionSource<bool> tcs)
		{
			try
			{
				act();
				tcs.SetResult(true);
			}
			catch (Exception e)
			{
				tcs.SetException(e);
			}
		}

		private static void ExecThenCompleteFuncTask<T>(Func<T> act, TaskCompletionSource<T> tcs)
		{
			try
			{
				var t = act();
				tcs.SetResult(t);
			}
			catch (Exception e)
			{
				// The exception is rethrown, catch there since sometimes this can happen in un-exceptional cases.
				//DebugOutput.DError($"Exception in ExecThenCompleteFuncTask: {e}");
				tcs.SetException(e);
			}
		}

		private static async Task ExecThenCompleteAsyncFuncTask<T>(Func<Task<T>> act, TaskCompletionSource<T> tcs)
		{
			try
			{
				var t = await act();
				tcs.SetResult(t);
			}
			catch (Exception e)
			{
				// The exception is rethrown, catch there since sometimes this can happen in un-exceptional cases.
				//DebugOutput.DError($"Exception in ExecThenCompleteFuncTask: {e}");
				tcs.SetException(e);
			}
		}

		public static Task ExecInMainThreadAsyncTask(this IUnityScheduler sched, Func<IEnumerable> a, [CallerMemberName] string taskName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			sched.ExecInMainThreadAsync(() => ExecThenCompleteContinuationTask(a, tcs), taskName);
			return tcs.Task;
		}

		private static IEnumerable ExecThenCompleteContinuationTask(Func<IEnumerable> act, TaskCompletionSource<bool> tcs)
		{
			IEnumerable iterable;
			IEnumerator iter;
			try
			{
				iterable = act();
				iter = iterable.GetEnumerator();
			}
			catch (Exception e)
			{
				tcs.SetException(e);
				yield break;
			}

			while (true)
			{
				try
				{
					if (!iter.MoveNext())
						break;
				}
				catch (Exception e)
				{
					tcs.SetException(e);
					yield break;
				}

				yield return iter.Current;
			}
			tcs.SetResult(true);
		}

		public static Task ExecInWorkerThreadTask(this IUnityScheduler sched, Func<Task> a, [CallerMemberName] string taskName = "")
		{
			if (sched is ImmediateOnlyUnityScheduler)
			{
				return a?.Invoke();
			}

			//if (Thread.CurrentThread.IsThreadPoolThread)
			//	return a();
			var task = Task.Run(a);
			task.ConfigureAwait(false);

			return task;
		}
		public static Task<T> ExecInWorkerThreadTask<T>(this IUnityScheduler sched, Func<Task<T>> a, [CallerMemberName] string taskName = "")
		{
			if (sched == null)
			{
				return a?.Invoke();
			}
			if (sched is ImmediateOnlyUnityScheduler)
			{
				return a?.Invoke();
			}

			//if (Thread.CurrentThread.IsThreadPoolThread)
			//	return a();

			var task = Task.Run(a);
			task.ConfigureAwait(false);

			return task;
		}

	}

	public static class UnityTaskExtensions
	{
		public static IEnumerator AsIEnumerator(this Task task)
		{
			while (!task.IsCompleted)
			{
				yield return null;
			}

			if (task.IsFaulted)
			{
				ExceptionDispatchInfo.Capture(task.Exception).Throw();
			}

			yield return null;
		}
	}
}