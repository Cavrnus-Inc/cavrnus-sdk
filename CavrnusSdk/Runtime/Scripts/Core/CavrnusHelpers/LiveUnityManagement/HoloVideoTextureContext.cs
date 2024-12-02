using System;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Holo;
using Collab.Proxy.Content;
using Collab.Proxy.Prop.ContainerContext;
using UnityBase.PropRenderers;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloVideoTextureContext : AAssetPropertyContext<TextureWithUVs>, ISpecifiedContentType, IExportableTextureContext
	{
		//TODO: BAD MEMORY USAGE???
		private TextureAssetHoloStreamComponent tc;
		private UnityVideoTextureImplementation uvti;

		private IDisposable implBinding;

		public HoloVideoTextureContext(UnityVideoTextureImplementation teximpl, ContentType contentType, TextureAssetHoloStreamComponent tc)
		{
			this.tc = tc;
			this.uvti = teximpl;
			implBinding = this.liveAsset.BindTo(uvti.Texture.Translating(t => new TextureWithUVs(t, new Rect(0, 0, 1, 1))));
			ContentType = contentType;
		}

		public void UpdateAsset(UnityVideoTextureImplementation teximpl)
		{
			implBinding?.Dispose();
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

		public ContentType ContentType { get; }

		public async Task ExportIntoTextureComponent(TextureAssetHoloStreamComponent component)
		{
			component.ImageFileName = tc.ImageFileName;
			component.ImageData = tc.ImageData;
			component.ImageFormat = tc.ImageFormat;
			component.TextureAvailable = tc.TextureAvailable;
			component.TextureCategory = tc.TextureCategory;
			component.TextureFilter = tc.TextureFilter;
		}
	}
}