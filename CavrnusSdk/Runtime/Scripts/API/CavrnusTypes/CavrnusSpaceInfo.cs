using System;

namespace CavrnusSdk.API
{
	public class CavrnusSpaceInfo
	{
		public string Name;
		public string Id;
		public DateTime LastAccessedTime;
		public string ThumbnailUrl;

		internal CavrnusSpaceInfo(string name, string id, DateTime lastAccessedTime, string thumbnailUrl)
		{
			Name = name;
			Id = id;
			LastAccessedTime = lastAccessedTime;
			ThumbnailUrl = thumbnailUrl;
		}
	}
}