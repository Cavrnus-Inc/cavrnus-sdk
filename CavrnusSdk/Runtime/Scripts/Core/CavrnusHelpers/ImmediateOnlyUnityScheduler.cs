using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Scheduler;
using UnityEngine;

namespace UnityBase
{
	public class ImmediateOnlyUnityScheduler : IUnityScheduler
	{
		public IScheduler BaseScheduler => new ImmediateOnlyScheduler();

		private MonoBehaviour coroutineRunner;

		public ImmediateOnlyUnityScheduler(MonoBehaviour coroutineRunner = null)
		{
			this.coroutineRunner = coroutineRunner;
		}

		public void Setup()
		{
		}

		public void ExecInMainThread(Action a, string taskName = "")
		{
			a?.Invoke();
		}

		public void ExecInMainThreadAsync(Action a, string taskName = "")
		{
			a?.Invoke();
		}

		public void ExecInMainThreadAsync(Func<IEnumerable> a, string taskName = "")
		{
			foreach (var eh in a())
			{
			}
		}

		public void ExecInMainThreadOrImmediate(Action a, string taskName = "")
		{
			a?.Invoke();
		}

		public void ExecInParallelThread(Action a, string taskName = "")
		{
			a?.Invoke();
		}

		public void ExecInParallelThread<T>(Func<T> parallel, Action<T> main, Action<Exception> finalize, string taskName = "")
		{
			try
			{
				var t = parallel.Invoke();
				main?.Invoke(t);
			}
			catch (Exception e)
			{
				finalize?.Invoke(e);
			}
		}

		public void ExecInMainThreadAfterDelay(float delay, Action action, string taskName = "")
		{
			action?.Invoke();
		}

		public void ExecInMainThreadAfterFrames(int frames, Action action, string taskName = "")
		{
			action?.Invoke();
		}

		public void ExecOnApplicationQuit(Action action, string taskName = "")
		{
		}

		public Coroutine ExecCoRoutine(IEnumerator coRoutine)
		{
			if (coroutineRunner == null)
			{
				throw new ApplicationException("Cannot run Coroutines through the immediate-only scheduler without a provided gameobject.");
			}

			return coroutineRunner.StartCoroutine(coRoutine);
		}

		public void CancelCoRoutine(Coroutine coRoutine)
		{
			if (coroutineRunner == null)
			{
				throw new ApplicationException("Cannot run Coroutines through the immediate-only scheduler without a provided gameobject.");
			}

			coroutineRunner.StopCoroutine(coRoutine);
		}

		public bool IsInMainThread()
		{
			return true;
		}
	}
}
