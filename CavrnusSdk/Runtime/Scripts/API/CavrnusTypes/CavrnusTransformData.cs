using System;
using UnityEngine;

namespace CavrnusSdk.API
{
	public class CavrnusTransformData
	{
		public Vector3 Position;
		public Vector3 EulerAngles;
		public Vector3 Scale;

		public CavrnusTransformData(Vector3 position, Vector3 eulerAngles, Vector3 scale)
		{
			Position = position;
			EulerAngles = eulerAngles;
			Scale = scale;
		}

		public override bool Equals(object obj)
		{
			return obj is CavrnusTransformData data &&
				   Position.Equals(data.Position) &&
				   EulerAngles.Equals(data.EulerAngles) &&
				   Scale.Equals(data.Scale);
		}

		public override int GetHashCode() { return HashCode.Combine(Position, EulerAngles, Scale); }
	}
}