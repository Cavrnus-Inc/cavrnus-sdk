using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBase
{
	internal class ExecInMainThreadRepeatingEachFrame : IDisposable
	{
		private Action task;
		private IUnityScheduler sched;
		private bool disposed;

		public ExecInMainThreadRepeatingEachFrame(Action task, IUnityScheduler sched)
		{
			this.task = task;
			this.sched = sched;

			Run();
		}

		private void Run()
		{
			if (disposed)
				return;
			task();
			sched.ExecInMainThreadAfterFrames(1, Run);
		}

		public void Dispose()
		{
			disposed = true;
		}
	}
}
