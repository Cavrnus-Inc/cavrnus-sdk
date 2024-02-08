using System;

namespace CavrnusSdk.API
{
	public class CavrnusOutputDevice
	{
		public string Name;
		internal string Id;

		internal CavrnusOutputDevice(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public override bool Equals(object obj)
		{
			return obj is CavrnusOutputDevice device &&
				   Name == device.Name &&
				   Id == device.Id;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}
	}
}