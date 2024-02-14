using Collab.Proxy.Comm.NotifyApi;

namespace CavrnusSdk.API
{
    public class CavrnusRemoteContent
    {
		public string Id => indo.Id;
		public string Name => indo.Name.Value;
        public string FileName => indo.Filename;
        public string ThumbnailUrl => indo.Thumbnail.Value.url;
        public Collab.Proxy.Comm.LiveTypes.ObjectCategoryEnum FileType =>  indo.Category.Value;

		internal INotifyDataObject indo;

        internal CavrnusRemoteContent(INotifyDataObject indo)
        {
            this.indo = indo;
        }
    }
}