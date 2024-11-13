using CavrnusCore;
using Collab.Base.Core;
using Collab.Proxy.Comm.NotifyApi;
using Collab.Proxy.Comm.RestApi.ObjectMetaTypes;
using Collab.Proxy.Content;
using System.Collections.Generic;
using System.Linq;

namespace CavrnusSdk.API
{
    public class CavrnusRemoteContent
    {
        public string Id => indo.Id;
        public string Name => indo.Name.Value;
        public string FileName => indo.Filename;
        public string ThumbnailUrl => indo.Thumbnail.Value.url;
        public long FileSize => indo.Assets?.FirstOrDefault(a => a.Value.assetCategory == ObjectAssetCategoryEnum.Canonical).Value?.length ?? 0;
        public string FileSizeString => FileSize.ToPrettySize();
        public bool CachedOnDisk => CavrnusStatics.ContentManager.ContentCache.TestIsInCache(CavrnusStatics.ContentManager.Endpoint.Server, indo.ToUoiDeprecateMe(), indo.Assets?.FirstOrDefault().Value, out string cachePath);

		public Dictionary<string, string> Tags 
        { get 
            {
				var dict = new Dictionary<string, string>();
				foreach (var item in indo.Metadata)
					dict[item.Key] = item.Value;
				return dict;
			}
        }

		internal INotifyDataObject indo;
        internal CavrnusRemoteContent(INotifyDataObject indo)
        {
            this.indo = indo;
        }
    }
}