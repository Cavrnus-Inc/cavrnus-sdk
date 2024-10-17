using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;

namespace Assets.com.cavrnus.csc.CavrnusSdk.Runtime.Scripts.Core
{
	public static class SubBindingExtension
	{
		public static IDisposable SubBind<T>(this IReadonlySetting<T> src, Func<T, IDisposable> func)
		{
			IDisposable internalhook = null;
			IDisposable srchook = src.Bind((v) =>
			{
				internalhook?.Dispose();
				internalhook = func(v);
			});

			return new DelegatedDisposalHelper(() =>
			{
				internalhook?.Dispose();
				internalhook = null;
				srchook?.Dispose();
				srchook = null;
			});
		}
	}
}
