using System;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using UnityBase.LiveUnityManagement;
using UnityEngine;

namespace UnityBase.PropRenderers
{

	public class UnityHoloCubeTextureImplementation : UnityCubeTextureImplementation, IContextProvidingPropertyRenderer
	{
		private PropertySetManager container;

		public UnityHoloCubeTextureImplementation(Cubemap tex) : base(tex)
		{
		}

		public override void SwapTexture(Cubemap tex)
		{
			base.SwapTexture(tex);

			if (this.container == null)
				return;
			var assetctx = this.container.FirstContextOfType<HoloCubeTextureContext>();
			assetctx.UpdateAsset(tex);
		}

		public IDisposable SetupContext(PropertySetManager container)
		{
			var contentType = new TextureContentType() { Usage = TextureContentType.TextureType.ColorMap };

			var d1 = container.AddContext(new HoloCubeTextureContext(this.tex, contentType));
			//var d2 = container.AddContext(new HoloCubeTextureThumbnailContext(this.tex));
			return d1; //.AlsoDispose(d2);
		}
	}
}