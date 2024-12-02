using System;
using Collab.Base.Math;
using Collab.Proxy.Prop.ContainerContext;
using UnityBase.PropRenderers;
using UnityBase.UnityServices;
using UnityEngine;

namespace UnityBase.LiveUnityManagement
{

	public class MaterialThumbnailContext : AThumbnailPropertyContext<TextureWithUVs>, IDisposable
	{
		private IThumbnailRenderer thumbnailService;
		private UnityMaterialImplementation unityMatImpl;
		private CountedBool fetchCount = new CountedBool();

		public MaterialThumbnailContext(UnityMaterialImplementation unityMatImpl, IThumbnailRenderer thumbnailService)
		{
			this.thumbnailService = thumbnailService;
			this.unityMatImpl = unityMatImpl;
		}

		protected override void Fetch()
		{
			if (!fetchCount.IsSet)
				unityMatImpl.materialChanged += ReRenderThumbnail;

			fetchCount.Set();
			ReRenderThumbnail(unityMatImpl.material);
		}

		protected override void Flush()
		{
			fetchCount.Unset();
			if (!fetchCount.IsSet)
				unityMatImpl.materialChanged -= ReRenderThumbnail;
		}

		private void ReRenderThumbnail(Material mat)
		{
			this.liveAsset.Value = new TextureWithUVs(thumbnailService.RenderMaterialThumbnail(mat));
		}

		public void Dispose()
		{
			Flush();
		}
	}
}