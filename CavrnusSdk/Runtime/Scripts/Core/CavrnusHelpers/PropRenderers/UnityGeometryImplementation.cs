using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Holo;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using UnityBase.LiveUnityManagement;
using UnityEngine;

namespace UnityBase.PropRenderers
{
	public class UnityGeometryImplementation : IGeometryPropertyRenderer, IContextProvidingPropertyRenderer
	{
		private PropertySetManager container;

		private Mesh liveMesh;

		public UnityGeometryImplementation(Mesh m)
		{
			this.liveMesh = m;
		}

		public void SwapMesh(Mesh mesh)
		{
			this.liveMesh = mesh;

			if (this.container == null)
				return;
			var assetctx = this.container.FirstContextOfType<HoloGeometryContext>();
			assetctx.UpdateAsset(mesh);
		}

		public IDisposable SetupContext(PropertySetManager container)
		{
			this.container = container;

			var d1 = container.AddContext(new HoloGeometryContext(this.liveMesh));
			return d1;
		}
	}
}
