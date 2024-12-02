using Collab.Proxy.Prop.ContainerContext;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloTextureThumbnailContext : AThumbnailPropertyContext<TextureWithUVs>
	{
		public HoloTextureThumbnailContext(Texture tex)
		{
			this.liveAsset.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
		}

		public void UpdateAsset(Texture tex)
		{
			this.liveAsset.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
		}

		protected override void Fetch()
		{
		}

		protected override void Flush()
		{
		}
	}
}