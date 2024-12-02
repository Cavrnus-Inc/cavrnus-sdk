using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBase
{
	internal class ExecInMainThreadRepeating : IDisposable
	{
		private float delay;
		private Action task;
		private IUnityScheduler sched;
		private bool disposed;

		public ExecInMainThreadRepeating(float delay, Action task, IUnityScheduler sched)
		{
			this.delay = delay;
			this.task = task;
			this.sched = sched;

			Run();
		}

		private void Run()
		{
			if (disposed)
				return;
            task();
			sched.ExecInMainThreadAfterDelay(delay, Run);
		}

		public void Dispose()
		{
			disposed = true;
		}
	}
}
