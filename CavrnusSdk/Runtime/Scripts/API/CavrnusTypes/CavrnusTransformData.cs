using System;
using UnityEngine;

namespace CavrnusSdk.API
{
	public class CavrnusTransformData
	{
		public Vector3 LocalPosition;
		public Vector3 LocalEulerAngles;
		public Vector3 LocalScale;

		public CavrnusTransformData(Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
		{
			LocalPosition = localPosition;
			LocalEulerAngles = localEulerAngles;
			LocalScale = localScale;
		}

		public override bool Equals(object obj)
		{
			return obj is CavrnusTransformData data &&
				   LocalPosition.Equals(data.LocalPosition) &&
				   LocalEulerAngles.Equals(data.LocalEulerAngles) &&
				   LocalScale.Equals(data.LocalScale);
		}

		public override int GetHashCode() { return HashCode.Combine(LocalPosition, LocalEulerAngles, LocalScale); }
	}
}