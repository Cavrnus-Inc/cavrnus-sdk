using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Holo;
using Collab.Proxy.Content;
using Collab.Proxy.Prop.ContainerContext;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class HoloTextureContext : AAssetPropertyContext<TextureWithUVs>, ISpecifiedContentType, IExportableTextureContext
	{
		//TODO: BAD MEMORY USAGE???
		private TextureAssetHoloStreamComponent tc;

		public HoloTextureContext(Texture tex, ContentType contentType, TextureAssetHoloStreamComponent tc)
		{
			this.tc = tc;
			this.liveAsset.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
			ContentType = contentType;
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