using System;
using Collab.Base.Core;
using Collab.Base.Math;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using Plane = Collab.Base.Math.Plane;
using Ray = Collab.Base.Math.Ray;

namespace UnityBase
{
	public static class TypeConverters
	{
		public static Vector2 ToVec2(this Float2 v)
		{
			return new Vector2(v.X, v.Y);
		}

		public static Float2 ToFloat2(this Vector2 v)
		{
			return new Float2(v.x, v.y);
		}

		public static Vector3 ToVec3(this Float3 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static Vector4 ToVec4(this Float3 v, float w)
		{
			return new Vector4(v.X, v.Y, v.Z, w);
		}
		public static Vector3 ToVec3(this Float4 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}
		public static Vector4 ToVec4(this Float4 v)
		{
			return new Vector4(v.X, v.Y, v.Z, v.W);
		}
		public static Quaternion ToQuat(this Float4 v)
		{
			return new Quaternion(v.X, v.Y, v.Z, v.w);
		}

		public static Float3 ToFloat3(this Vector3 v)
		{
			return new Float3(v.x, v.y, v.z);
		}

		public static Float3 ToFloat3(this Vector4 v)
		{
			return new Float3(v.x, v.y, v.z);
		}

		public static Float4 ToFloat4(this Vector3 v)
		{
			return new Float4(v.x, v.y, v.z, 0);
		}
		public static Float4 ToFloat4(this Vector3 v, float w)
		{
			return new Float4(v.x, v.y, v.z, w);
		}
		public static Float4 ToFloat4(this Vector4 v)
		{
			return new Float4(v.x, v.y, v.z, v.w);
		}
		public static Float4 ToFloat4(this Quaternion v)
		{
			return new Float4(v.x, v.y, v.z, v.w);
		}

		public static Ray ToCollabRay(this UnityEngine.Ray r)
		{
			return new Ray(r.origin.ToFloat3(), r.direction.ToFloat3());
		}

		public static UnityEngine.Ray ToUnityRay(this Ray r)
		{
			return new UnityEngine.Ray(r.Start.ToVec3(), r.Direction.ToVec3());
		}

		public static Color ToColor(this Color4F c)
		{
			return new Color(c.r, c.g, c.b, c.a);
		}

		public static Color ToColor(this Color4 c4)
		{
			var c = c4.ToColor4F();
			return new Color(c.r, c.g, c.b, c.a);
		}

		public static Color32 ToColor32(this Color4 c)
		{
			return new Color32(c.r, c.g, c.b, c.a);
		}

		public static Color4 ToColor4(this Color c)
		{
			return new Color4(c.r, c.g, c.b, c.a);
		}

		public static Vector3 ToHSV(this Color c)
		{
			float hColor, sColor, vColor;
			Color.RGBToHSV(c, out hColor, out sColor, out vColor);
			return new Vector3(hColor, sColor, vColor);
		}

		public static Color4F ToColor4F(this Color c)
		{
			return new Color4F(c.r, c.g, c.b, c.a);
		}

		public static Float4 ToFloat4(this Color4F c)
		{
			return new Float4() { X = c.R, Y = c.G, Z = c.B, W = c.A };
		}

		public static Color4F ToColor4F(this Float4 f)
		{
			return new Color4F(f.X, f.Y, f.Z, f.W);
		}

		public static Matrix34 ToMatrix34(this Matrix4x4 mtx)
		{
			return new Matrix34(mtx.GetColumn(0).ToFloat3(), mtx.GetColumn(1).ToFloat3(), mtx.GetColumn(2).ToFloat3(), mtx.GetColumn(3).ToFloat3());
		}

		public static Plane ToCollabPlane(this UnityEngine.Plane p)
		{
			var result = new Plane();
			result.Normal = p.normal.ToFloat3();
			result.Distance = p.distance;
			return result;
		}

		public static UnityEngine.Plane ToUnityPlane(this Plane p)
		{
			var result = new UnityEngine.Plane();
			result.normal = p.Normal.ToVec3();
			result.distance = p.Distance;
			return result;
		}

		public static AxisAlignedBoundingBox ToAABB(this Bounds b)
		{
			return new AxisAlignedBoundingBox(b.min.ToFloat3(), b.max.ToFloat3());
		}

		public static Bounds ToBounds(this AxisAlignedBoundingBox aabb)
		{
			Bounds b = new Bounds();
			b.min = aabb.min.ToVec3();
			b.max = aabb.max.ToVec3();
			return b;
		}
	}

	public static class BufferConverters
	{
		public static unsafe Vector3[] UnsafeCopyTranslateVector3(Float3[] fs)
		{
			Vector3[] dest = new Vector3[fs.Length];
			try
			{
				fixed (void* srcptr = fs)
				{
					fixed (void* dstptr = dest)
					{
						Buffer.MemoryCopy(srcptr, dstptr, fs.Length * sizeof(Float3), fs.Length * sizeof(Float3));
					}
				}
				return dest;
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error while copying F3[] to V3[]: " + e);
				return null;
			}
		}
		public static unsafe Vector4[] UnsafeCopyTranslateVector4(Float4[] fs)
		{
			Vector4[] dest = new Vector4[fs.Length];
			try
			{
				fixed (void* srcptr = fs)
				{
					fixed (void* dstptr = dest)
					{
						Buffer.MemoryCopy(srcptr, dstptr, fs.Length * sizeof(Float4), fs.Length * sizeof(Float4));
					}
				}
				return dest;
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error while copying F4[] to V4[]: " + e);
				return null;
			}
		}
		public static unsafe Vector2[] UnsafeCopyTranslateVector2(Float2[] fs)
		{
			Vector2[] dest = new Vector2[fs.Length];
			try
			{
				fixed (void* srcptr = fs)
				{
					fixed (void* dstptr = dest)
					{
						Buffer.MemoryCopy(srcptr, dstptr, fs.Length * sizeof(Float2), fs.Length * sizeof(Float2));
					}
				}
				return dest;
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error while copying F2[] to V2[]: " + e);
				return null;
			}
		}
		public static unsafe Color32[] UnsafeCopyTranslateColor4(Color4[] cs)
		{
			Color32[] dest = new Color32[cs.Length];
			try
			{
				fixed (void* srcptr = cs)
				{
					fixed (void* dstptr = dest)
					{
						Buffer.MemoryCopy(srcptr, dstptr, cs.Length * sizeof(Color4), cs.Length * sizeof(Color4));
					}
				}
				return dest;
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error while copying C4[] to C32[]: " + e);
				return null;
			}
		}
		public static unsafe int[] UnsafeCopyTranslateInt3(Int3[] v)
		{
			int[] dest = new int[v.Length * 3];
			try
			{
				fixed (void* srcptr = v)
				{
					fixed (void* dstptr = dest)
					{
						Buffer.MemoryCopy(srcptr, dstptr, v.Length * sizeof(Int3), v.Length * sizeof(Int3));
					}
				}
				return dest;
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error while copying I3[] to int[]: " + e);
				return null;
			}
		}
	}
}