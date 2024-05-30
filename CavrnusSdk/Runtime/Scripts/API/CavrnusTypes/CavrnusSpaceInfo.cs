using Collab.Proxy.Comm.NotifyApi;
using System;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceInfo
	{
		public string Name => room.Name.Value;
		public string Id => room.Id;
		public DateTime LastAccessedTime => room.ConnectedMember?.Value?.LastAccess?.Value ?? new DateTime(0L, DateTimeKind.Utc);
		public string ThumbnailUrl => room.ThumbnailUrl.Value?.ToString();

		private INotifyDataRoom room;

		internal CavrnusSpaceInfo(INotifyDataRoom room)
		{
			this.room = room;
		}
	}
}