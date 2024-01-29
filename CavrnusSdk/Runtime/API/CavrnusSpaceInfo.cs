using Collab.Proxy.Comm.RestApi;
using System;

namespace CavrnusSdk
{
	public class CavrnusSpaceInfo
	{
		public string Name => roomMeta.name;
		public string Id => roomMeta._id;
		public DateTime LastAccessedTime => roomMeta.modifiedAt;

		private RoomMetadataRest roomMeta;
		public CavrnusSpaceInfo(RoomMetadataRest roomMeta) { this.roomMeta = roomMeta; }
	}
}