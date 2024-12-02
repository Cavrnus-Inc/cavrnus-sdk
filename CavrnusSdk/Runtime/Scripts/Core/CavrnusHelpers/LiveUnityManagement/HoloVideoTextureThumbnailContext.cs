using System;
using Collab.Base.Collections;
using Collab.Proxy.Prop.ContainerContext;
using UnityBase.PropRenderers;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloVideoTextureThumbnailContext : AThumbnailPropertyContext<TextureWithUVs>
	{
		private UnityVideoTextureImplementation uvti;
		private IDisposable implBinding;

		public HoloVideoTextureThumbnailContext(UnityVideoTextureImplementation teximpl)
		{
			this.uvti = teximpl;
			implBinding = this.liveAsset.BindTo(uvti.Texture.Translating(t => new TextureWithUVs(t, new Rect(0, 0, 1, 1))));
		}

		protected override void Fetch()
		{
		}

		protected override void Flush()
		{
			implBinding?.Dispose();
			implBinding = null;
		}

	}
}