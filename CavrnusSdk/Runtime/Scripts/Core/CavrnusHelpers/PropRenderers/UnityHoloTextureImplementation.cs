using System;
using Collab.Base.Core;
using Collab.Holo;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using UnityBase.LiveUnityManagement;
using UnityEngine;

namespace UnityBase.PropRenderers
{

	public class UnityHoloTextureImplementation : UnityTextureImplementation, IContextProvidingPropertyRenderer
	{
		private TextureAssetHoloStreamComponent srcComponent;

		private PropertySetManager container;

		public UnityHoloTextureImplementation(Texture tex, TextureAssetHoloStreamComponent c) : base(tex)
		{
			this.srcComponent = c;
		}

		public override void SwapTexture(Texture tex)
		{
			base.SwapTexture(tex);

			if (this.container == null)
				return;
			var assetctx = this.container.FirstContextOfType<HoloTextureContext>();
			assetctx.UpdateAsset(tex);
			var thumbctx = this.container.FirstContextOfType<HoloTextureThumbnailContext>();
			thumbctx.UpdateAsset(tex);
		}


		public IDisposable SetupContext(PropertySetManager container)
		{
			this.container = container;

			var contentType = new TextureContentType() { Usage = srcComponent.TextureCategory.ToTextureContentType() };

			var d1 = container.AddContext(new HoloTextureContext(this.tex, contentType, srcComponent));
			var d2 = container.AddContext(new HoloTextureThumbnailContext(this.tex));
			return d1.AlsoDispose(d2);
		}
	}
}