using Collab.Proxy.Comm.NotifyApi;
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