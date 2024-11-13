using Collab.Proxy.Comm.NotifyApi;
using System;
using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceInfo
	{
		public string Name{ get; private set; }
		public string Id{ get; private set; }
		public DateTime LastAccessedTime{ get; private set; }
		public string ThumbnailUrl{ get; private set; }

		internal CavrnusSpaceInfo(RoomMetadataRest roomMetadataRest)
		{
			Name = roomMetadataRest.name;
			Id = roomMetadataRest._id;
			LastAccessedTime = roomMetadataRest.modifiedAt;
			ThumbnailUrl = roomMetadataRest.thumbnailContentUrl;
		}

		internal CavrnusSpaceInfo(INotifyDataRoom room)
		{
			Name = room.Name.Value;
			Id = room.Id;
			LastAccessedTime = room.ConnectedMember?.Value?.LastAccess?.Value ?? new DateTime(0L, DateTimeKind.Utc);
			ThumbnailUrl = room.ThumbnailUrl.Value?.ToString();
		}
	}
}