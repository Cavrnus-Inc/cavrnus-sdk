using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Holo;
using Collab.Proxy.Content;
using Collab.Proxy.Prop.ContainerContext;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloGeometryContext : AAssetPropertyContext<Mesh>
	{
		public HoloGeometryContext(Mesh mesh)
		{
			this.liveAsset.Value = mesh;
		}

		public void UpdateAsset(Mesh mesh)
		{
			this.liveAsset.Value = mesh;
		}

		protected override void Fetch()
		{
		}

		protected override void Flush()
		{
		}
	}
}