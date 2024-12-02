using Collab.Proxy.Content;
using Collab.Proxy.Prop.ContainerContext;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloMaterialContext : AAssetPropertyContext<Material>, ISpecifiedContentType
	{
		public HoloMaterialContext(Material mat)
		{
			this.liveAsset.Value = mat;
		}

		protected override void Fetch()
		{
		}

		protected override void Flush()
		{
		}

		public ContentType ContentType { get; } = new MaterialContentType() { Usage = MaterialContentType.MaterialType.Unspecified };
	}
}