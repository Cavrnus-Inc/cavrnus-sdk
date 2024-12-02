using Collab.Proxy.Content;
using Collab.Proxy.Prop.ContainerContext;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloCubeTextureContext : AAssetPropertyContext<TextureWithUVs>, ISpecifiedContentType
	{
		//TODO: BAD MEMORY USAGE???
		public HoloCubeTextureContext(Cubemap tex, ContentType contentType)
		{
			this.liveAsset.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
			ContentType = contentType;
		}

		public void UpdateAsset(Cubemap tex)
		{
			this.liveAsset.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
		}

		protected override void Fetch()
		{
		}

		protected override void Flush()
		{
		}

		public ContentType ContentType { get; }
	}
}