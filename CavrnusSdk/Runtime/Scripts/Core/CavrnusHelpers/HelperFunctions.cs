using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Proxy.Prop.TransformProp;
using UnityBase;
using UnityBase.Objects;
using UnityEngine;
using Plane = UnityEngine.Plane;

namespace UnityBase
{
	public static class HelperFunctions
	{
		public static TransformDataSRT ToSrt(this Transform t)
		{
			return new TransformDataSRT()
			{
				translation = t.localPosition.ToFloat3(),
				euler = t.localEulerAngles.ToFloat3(),
				scale = t.localScale.ToFloat3(),
			};
		}

		public static bool IsBefore(this DateTime date, DateTime otherDate)
		{
			return DateTime.Compare(date, otherDate) < 0;
		}

		public static bool SubstringSafe(this string str, int start, int length)
		{
			return start >= 0 && start < str.Length && start + length >= 0 && start + length <= str.Length;
		}

		public static string GetSafeSubstring(this string str, int start, int length)
		{
			if (str.SubstringSafe(start, length))
				return str.Substring(start, length);
			return "";
		}

		public static string ToLocalHoursOnDateString(this DateTime time)
		{
			DateTime localTime = time.ToLocalTime();
			return localTime.ToString("hh:mm tt") + " on " + localTime.ToString("M/dd/yyyy");
		}

		public static Float3 RGBToHSV(this Color c)
		{
			float h, s, v;
			Color.RGBToHSV(c, out h, out s, out v);
			return new Float3(h, s, v);
		}

		public static Float3 ToRGB255Space(this Color c)
		{
			return new Float3(c.r * 255f, c.g * 255f, c.b * 255f);
		}

		public static Color FromRGB255Space(this Float3 c)
		{
			return new Color(c.X / 255f, c.Y / 255f, c.Z / 255f);
		}

		public static string RemoveInvalidFileFolderNameCharacters(this string str)
		{
			/*
			https://stackoverflow.com/questions/1976007/what-characters-are-forbidden-in-windows-and-linux-directory-names
			* < (less than)
				> (greater than)
				: (colon - sometimes works, but is actually NTFS Alternate Data Streams)
				" (double quote)
				/ (forward slash)
				\ (backslash)
				| (vertical bar or pipe)
				? (question mark)
				* (asterisk)
			*/
			string[] invalidChars = { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };

			string newStr = str;
			foreach (var c in invalidChars)
			{
				newStr = newStr.Replace(c, "");
			}
			return newStr.Trim().Trim('.');
		}

		public static Action<T> Wrap<T>(this Action<T> action, Action<T> otherAction)
		{
			return obj =>
			{
				action?.Invoke(obj);
				otherAction?.Invoke(obj);
			};
		}

		public static bool EqualsMat(this Material mat1, Material mat2)
		{
			if (mat1.shader != mat2.shader)
				return false;

			if (mat1.HasProperty("_Color") && mat2.HasProperty("_Color") && mat1.color != mat2.color)
				return false;

			if (mat1.name != mat2.name)
				return false;

			if (mat1.HasProperty("_MainTex") && mat2.HasProperty("_MainTex") && mat1.mainTexture != mat2.mainTexture)
				return false;

			if (mat1.HasProperty("_SpecColor") && mat2.HasProperty("_SpecColor") && mat1.GetColor("_SpecColor") != mat2.GetColor("_SpecColor"))
				return false;

			if (mat1.HasProperty("_SpecularColor") && mat2.HasProperty("_SpecularColor") && mat1.GetColor("_SpecularColor") != mat2.GetColor("_SpecularColor"))
				return false;

			if (mat1.HasProperty("_ReflectionColor") && mat2.HasProperty("_ReflectionColor") && mat1.GetColor("_ReflectionColor") != mat2.GetColor("_ReflectionColor"))
				return false;

			if (mat1.HasProperty("_TransmissionColor") && mat2.HasProperty("_TransmissionColor") && mat1.GetColor("_TransmissionColor") != mat2.GetColor("_TransmissionColor"))
				return false;

			return true;
		}

		public static bool HasComponent<T>(this GameObject obj)
		{
			return obj.GetComponent<T>() != null;
		}

		public static bool MatInDictionary<T>(this Dictionary<Material, T> matDict, Material mat)
		{
			foreach (KeyValuePair<Material, T> pair in matDict)
			{
				if (mat.EqualsMat(pair.Key))
					return true;
			}
			return false;
		}

		public static T GetMatFromDictionary<T>(this Dictionary<Material, T> matDict, Material mat)
			where T : class
		{
			foreach (KeyValuePair<Material, T> pair in matDict)
			{
				if (mat.EqualsMat(pair.Key))
					return pair.Value;
			}
			return null;
		}
		
		public static Color GetMatColor(this Material mat)
		{
			int hexColorBrightness;
			bool useSpecularColor = mat.HasProperty("_SpecularColor") && !Int32.TryParse(ColorUtility.ToHtmlStringRGBA(mat.GetColor("_SpecularColor"))[0].ToString(), out hexColorBrightness);

			if (mat.HasProperty("_TransmissionColor") && mat.shader.name == "Custom/ColoredGlass")
				return mat.GetColor("_TransmissionColor");
			else if (mat.HasProperty("_ReflectionColor") && mat.shader.name == "Custom/Glass")
				return mat.GetColor("_ReflectionColor");
			else if (mat.HasProperty("_SpecularColor") && useSpecularColor)
				return mat.GetColor("_SpecularColor");
			else if (mat.HasProperty("_Color"))
				return mat.GetColor("_Color");
			else if (mat.HasProperty("_TintColor"))
				return mat.GetColor("_TintColor");
			else if (mat.HasProperty("_SpecColor"))
				return mat.GetColor("_SpecColor");
			else if (mat.HasProperty("_BaseColor"))
				return mat.GetColor("_BaseColor");
			else return new Color(0f, 0f, 0f, 0f);
		}

		public static void SetMatColor(this Material mat, Color c)
		{
			if (mat.HasProperty("_Color"))
				mat.SetColor("_Color", c);
			else if (mat.HasProperty("_MainColor"))
				mat.SetColor("_MainColor", c);
			else if (mat.HasProperty("_TintColor"))
				mat.SetColor("_TintColor", c);
			else if (mat.HasProperty("_SpecColor"))
				mat.SetColor("_SpecColor", c);
			else if (mat.HasProperty("_BaseColor"))
				mat.SetColor("_BaseColor", c);
		}

		public static Vector2 GetMatScale(this Material mat)
		{
			if (mat.HasProperty("_MainTex"))
				return mat.GetTextureScale("_MainTex");
			return Vector2.one;
		}

		public static void SetMatScale(this Material mat, Vector2 newScale)
		{
			if (mat.HasProperty("_MainTex"))
				mat.SetTextureScale("_MainTex", newScale);
		}

		public static Vector2 GetMatOffset(this Material mat)
		{
			if (mat.HasProperty("_MainTex"))
				return mat.GetTextureOffset("_MainTex");
			return Vector2.one;
		}

		public static void SetMatOffset(this Material mat, Vector2 newOffset)
		{
			if (mat.HasProperty("_MainTex"))
				mat.SetTextureOffset("_MainTex", newOffset);
		}

		public static Texture GetMatTexture(this Material mat)
		{
			if (mat.HasProperty("_MainTex"))
				return mat.GetTexture("_MainTex");
			return null;
		}

		public static void SetMatTexture(this Material mat, Texture newTex)
		{
			if (mat.HasProperty("_MainTex"))
				mat.SetTexture("_MainTex", newTex);
		}

		public static void SetMatFlip(this Material mat, bool flip, Vector2 regularOffset, Vector2 regularScale)
		{
			if (mat.HasProperty("_MainTex"))
			{
				if (flip)
				{
					mat.mainTextureOffset = new Vector2(0, 1);
					mat.mainTextureScale = new Vector2(1, -1f);
				}
				else
				{
					mat.mainTextureOffset = regularOffset;
					mat.mainTextureScale = regularScale;
				}
			}
		}

		public static Vector3 VectorTo(this Vector3 a, Vector3 b)
		{
			return b - a;
		}

		public static Vector3 Abs(this Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		public static Transform FindDeepChild(this Transform aParent, string aName)
		{
			foreach (Transform child in aParent)
			{
				if (child.name == aName)
					return child;
				var result = child.FindDeepChild(aName);
				if (result != null)
					return result;
			}
			return null;
		}

		public static Vector3 YIsZero(this Vector3 vec3)
		{
			return new Vector3(vec3.x, 0, vec3.z);
		}

		public static T GetComponentInAllParents<T>(this GameObject obj)
			where T : class
		{
			if (HelperFunctions.NullOrDestroyed(obj))
				return null;

			T result = obj.transform.GetComponent<T>();
			if (!NullOrDestroyed(result))
			{
				return result;
			}
			else if (obj.transform.parent == null)
			{
				return null;
			}
			else return obj.transform.parent.gameObject.GetComponentInAllParents<T>();
		}

		public static T GetComponentInAllChildrenOnly<T>(this GameObject obj)
			where T : class
		{
			if (HelperFunctions.NullOrDestroyed(obj))
				return null;

			if (obj.transform.childCount == 0)
				return null;
			else
			{
				T result = obj.transform.GetChild(0).GetComponentInChildren<T>();
				if (!NullOrDestroyed(result))
					return result;
				else return obj.transform.GetChild(0).GetComponentInChildren<T>();
			}
		}

		public static List<T> GetComponentsInAllParents<T>(this GameObject obj)
			where T : class
		{
			List<T> result = obj.transform.GetComponents<T>().ToList();

			if (obj.transform.parent == null)
			{
				return result;
			}
			else return result.Concat(obj.transform.parent.gameObject.GetComponentsInAllParents<T>()).ToList();
		}

		public static List<T> GetComponentsOnlyInImmediateChildren<T>(this GameObject obj)
			where T : class
		{
			List<T> result = new List<T>();
			foreach (var child in obj.transform.GetAllChildren())
			{
				result = result.Concat(child.GetComponents<T>()).ToList();
			}
			return result;
		}

		public static Texture2D BuildTextureFromImage2D(Image2D im)
		{
			Texture2D t = new Texture2D(im.Resolution.X, im.Resolution.Y, TextureFormat.ARGB32, true);
			for (int i = 0; i < im.Resolution.Y; i++)
			{
				for (int j = 0; j < im.Resolution.X; j++)
				{
					t.SetPixel(j, i, im.GetPixel(j, i).ToColor32());
				}
			}
			t.Apply(true);
			return t;
		}

		private static Bounds EMPTYBOUNDS = new Bounds();
		public static Bounds GetHierarchyBounds(this GameObject obj)
		{
			Bounds baseBounds = new Bounds();

			foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>(true))
			{
				Bounds b = rend.bounds;
				if (b == EMPTYBOUNDS)
					continue;
				if (rend is ParticleSystemRenderer)
				{
					b = new Bounds(b.center, b.size * .5f);
				}

				if (baseBounds == EMPTYBOUNDS)
					baseBounds = b;
				else 
					baseBounds.Encapsulate(b);
			}
			return baseBounds;
		}

		public static AxisAlignedBoundingBox GetRelativeHierarchyBounds(this GameObject obj)
		{
			AxisAlignedBoundingBox r = AxisAlignedBoundingBox.Empty;

			var atRootMtxInv = obj.transform.parent?.transform?.worldToLocalMatrix ?? Matrix4x4.identity;
			foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>(true))
			{
				if (!rend.enabled)
					continue;

				var worldBounds = rend.bounds.ToAABB();
				worldBounds.GetCorners().ForEach(c => r.ExpandToInclude(atRootMtxInv.MultiplyPoint(c.ToVec3()).ToFloat3()));
			}
			return r;
		}

		public static AxisAlignedBoundingBox GetRelativeHierarchyBoundsAtLevel(this GameObject obj)
		{
			AxisAlignedBoundingBox r = AxisAlignedBoundingBox.Empty;

			var atRootMtxInv = obj.transform.worldToLocalMatrix;
			foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>(true))
			{
				if (rend is ParticleSystemRenderer)
					continue;
				if(rend.GetComponent<MeshFilter>() == null)
				{
					//DebugOutput.Info($"Could not find mesh renderer on component {rend.gameObject.name}.  We assumed one was there because of the {rend}");
					continue;
				}
				if (rend.GetComponent<MeshFilter>().sharedMesh == null)
				{
					//DebugOutput.Info($"Could not find mesh attached to MeshFilter component {rend.gameObject.name}.  We assumed one was there because of the {rend}");
					continue;
				}
				var localBounds = rend.GetComponent<MeshFilter>().sharedMesh.bounds.ToAABB();
				var worldmtx = rend.transform.localToWorldMatrix;
				localBounds.GetCorners().ForEach(c => r.ExpandToInclude(atRootMtxInv.MultiplyPoint(worldmtx.MultiplyPoint(c.ToVec3())).ToFloat3()));
			}
			return r;
		}

		public static string HierarchyToString(this Transform t)
		{
			return HierarchyToStringImpl(t, "");
		}

		private static string HierarchyToStringImpl(this Transform t, string res)
		{
			if (t == null)
				return res;

			if (String.IsNullOrEmpty(res))
				return HierarchyToStringImpl(t.parent, $"{t.name}");
			else
				return HierarchyToStringImpl(t.parent, $"{t.name}/{res}");
		}

		public static Float3 SetY(this Float3 f3, float y)
		{
			return new Float3(f3.X, y, f3.Z);
		}

		public static Vector3 GetYVec(this Vector3 v3)
		{
			return new Vector3(0f, v3.y, 0f);
		}

		public static bool InRangeOfVal(this int valToCheck, double val, double range)
		{
			return valToCheck >= (val - range) && valToCheck <= (val + range);
		}

		public static bool CanRead(string path)
		{
			try
			{
				if (Directory.Exists(path))
				{
					Directory.GetFiles(path);
					return true;
				}
				if (File.Exists(path))
				{
					FileStream fs = File.OpenRead(path);
					fs.Close();
					return true;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsDeepChildOf(this GameObject child, GameObject parent)
		{
			if (child == parent)
				return true;
			if (child.transform.parent != null)
				return IsDeepChildOf(child.transform.parent.gameObject, parent);
			return false;
		}

		public static Transform[] GetAllChildren(this Transform parent)
		{
			Transform[] children = new Transform[parent.childCount];
			for (int i = 0; i < parent.childCount; i++)
			{
				children[i] = parent.GetChild(i);
			}
			return children;
		}

		public static bool RequiresServerProcessing(string extension)
		{
			return false; //SupportedFileTypes.GetFileCategory(extension) == SupportedFileTypes.FileTypeCategoryEnum.VideoContent;
		}

		public static bool AlmostEquals(this float f1, float f2, float epsilon = Single.Epsilon)
		{
			return (f1 - epsilon <= f2) && (f1 + epsilon >= f2);
		}

		public static bool AlmostEquals(this Vector3 f1, Vector3 f2, float epsilon = Single.Epsilon)
		{
			return f1.x.AlmostEquals(f2.x, epsilon) && f1.y.AlmostEquals(f2.y, epsilon) && f1.z.AlmostEquals(f2.z, epsilon);
		}


		//From: http://answers.unity3d.com/questions/53989/test-to-see-if-a-vector3-point-is-within-a-boxcoll.html
		public static bool PointWithinOBB(Vector3 point, BoxCollider box)
		{
			Vector3 localBoxPoint = box.transform.InverseTransformPoint(point) - box.center;
			//TODO: adjust for lossy scale
			float halfX = (box.size.x * 0.5f);
			float halfY = (box.size.y * 0.5f);
			float halfZ = (box.size.z * 0.5f);

			return localBoxPoint.x < halfX && localBoxPoint.x > -halfX &&
					localBoxPoint.y < halfY && localBoxPoint.y > -halfY &&
					localBoxPoint.z < halfZ && localBoxPoint.z > -halfZ;
		}
		//END From unity forums

		public static List<Plane> GetPlanesFromBox(BoxCollider box)
		{
			List<Plane> planes = new List<Plane>();
			Vector3 boxWorldScale = box.transform.lossyScale;
			Vector3 boxPlaneLocalOffsets = new Vector3((box.size.x / 2) * boxWorldScale.x, (box.size.y / 2) * boxWorldScale.y, (box.size.z / 2) * boxWorldScale.z);

			planes.Add(new Plane(box.transform.up, box.transform.TransformPoint(box.center + new Vector3(0, boxPlaneLocalOffsets.y, 0))));
			planes.Add(new Plane(-box.transform.up, box.transform.TransformPoint(box.center - new Vector3(0, boxPlaneLocalOffsets.y, 0))));
			planes.Add(new Plane(box.transform.right, box.transform.TransformPoint(box.center + new Vector3(boxPlaneLocalOffsets.x, 0, 0))));
			planes.Add(new Plane(-box.transform.right, box.transform.TransformPoint(box.center - new Vector3(boxPlaneLocalOffsets.x, 0, 0))));
			planes.Add(new Plane(box.transform.forward, box.transform.TransformPoint(box.center + new Vector3(0, 0, boxPlaneLocalOffsets.z))));
			planes.Add(new Plane(-box.transform.forward, box.transform.TransformPoint(box.center - new Vector3(0, 0, boxPlaneLocalOffsets.z))));
			return planes;
		}

		public static Vector3 IntersectOBBFromInterior(UnityEngine.Ray ray, List<Plane> planes)
		{
			float shortestDistanceHit = 0;
			bool shortestDistanceSet = false;
			foreach (var plane in planes)
			{
				float planeDist;
				if (plane.Raycast(ray, out planeDist))
				{
					if (!shortestDistanceSet || planeDist < shortestDistanceHit)
					{
						shortestDistanceSet = true;
						shortestDistanceHit = planeDist;
					}
				}
			}
			return ray.GetPoint(shortestDistanceHit);
		}

		public static GameObject FindObject(GameObject parent, string name)
		{
			Transform[] trs = parent.GetComponentsInChildren<Transform>();
			foreach (Transform t in trs)
			{
				if (t.name == name)
				{
					return t.gameObject;
				}
			}
			return null;
		}

		public static string toBetterString(this Vector3 v)
		{
			return "(" + v.x + "," + v.y + "," + v.z + ")";
		}

		public static string toBetterString(this Vector2 v)
		{
			return "(" + v.x + "," + v.y + ")";
		}

		public static bool NameRefersToCameraStream(string name)
		{
			if (name == null)
				return false;
			return name.ToLowerInvariant().StartsWith("camera: '");
		}

		public static bool NameRefersToSmartStream(string name)
		{
			if (name == null)
				return false;
			return name.ToLowerInvariant().StartsWith("smartstream");
		}

		public static Vector3 FromTo(this Vector3 from, Vector3 to)
		{
			return to - from;
		}

		public static Vector2 FromTo(this Vector2 from, Vector2 to)
		{
			return to - from;
		}

		public static Vector3 AxisMask(this Vector3 vector, Vector3 axis)
		{
			return new Vector3(vector.x * axis.x, vector.y * axis.y, vector.z * axis.z);
		}

		public static float LinearDistFromCamera(Camera cam, Vector3 pos)
		{
			if (cam.orthographic)
				return cam.orthographicSize;
			return Vector3.Dot(cam.transform.forward, cam.transform.position.FromTo(pos));
		}

		public static float ViewHeightAtPoint(Camera cam, Vector3 pos)
		{
			return 2f * LinearDistFromCamera(cam, pos) * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
		}
		public static float ViewHeightAtDistance(Camera cam, float distance)
		{
			return 2f * distance * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
		}
		public static float ViewWidthAtPoint(Camera cam, Vector3 pos, float aspectRatio)
		{
			return aspectRatio * 2f * LinearDistFromCamera(cam, pos) * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
		}
		public static float ViewWidthAtDistance(Camera cam, float distance, float aspectRatio)
		{
			return aspectRatio * 2f * distance * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
		}

		//Unity is dumb (.Equals(null) checks if destroyed)
		public static bool NullOrDestroyed<T>(T o)
		{
			return o == null || o.Equals(null);
		}

		public static Transform TryGetChild(this Transform t, int childIndex)
		{
			if (childIndex < 0 || childIndex >= t.childCount)
				return null;
			return t.GetChild(childIndex);
		}

		public static Texture2D CreateReadableTexture2D(Texture2D source)
		{
			RenderTexture renderTex = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.Default);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}

		public static bool EqualsEpsilon(this Vector3 vec, Vector3 cmp, float epsilon)
		{
			return vec.x.EqualsEpsilon(cmp.x, epsilon) &&
					vec.y.EqualsEpsilon(cmp.y, epsilon) &&
					vec.z.EqualsEpsilon(cmp.z, epsilon);
		}

		public static bool AngleEpsilon(this Vector3 vec, Vector3 cmp, float epsilon)
		{
			return vec.x.AngleEpsilon(cmp.x, epsilon) &&
					vec.y.AngleEpsilon(cmp.y, epsilon) &&
					vec.z.AngleEpsilon(cmp.z, epsilon);
		}

		public static bool AngleEpsilon(this float f, float cmp, float epsilon)
		{
			f = f % 360f;
			if (f < 0)
				f = 360 + f;
			cmp = cmp % 360f;
			if (cmp < 0)
				cmp = 360 + cmp;

			var diff = 180 - Mathf.Abs(Mathf.Abs(f - cmp) - 180);

			return diff <= epsilon;
		}

		public static bool LessThan(this float val, float other)
		{
			return val < other - 0.0001f;
		}

		public static bool GreaterThan(this float val, float other)
		{
			return val > other + 0.0001f;
		}

		public static TranslatingSetting<bool, bool> Inverse(this IReadonlySetting<bool> setting)
		{
			return new TranslatingSetting<bool, bool>(setting, b => !b);
		}

		public static MultiWrappedSettingAnd AndSetting(this IReadonlySetting<bool> setting, IReadonlySetting<bool> setting2, string name = null)
		{
			return new MultiWrappedSettingAnd(name, setting, setting2);
		}

		public static MultiWrappedSettingOr OrSetting(this IReadonlySetting<bool> setting, IReadonlySetting<bool> setting2)
		{
			return new MultiWrappedSettingOr(setting, setting2);
		}

		public static TranslatingSetting<string, bool> IsNullOrWhitespace(this IReadonlySetting<string> setting)
		{
			return new TranslatingSetting<string, bool>(setting, s => String.IsNullOrWhiteSpace(s));
		}

		public static TranslatingSetting<string, bool> NotNullOrWhitespace(this IReadonlySetting<string> setting)
		{
			return new TranslatingSetting<string, bool>(setting, s => !String.IsNullOrWhiteSpace(s));
		}

		public static IReadonlySetting<bool> Wrap(this IReadonlySetting<bool> setting)
		{
			return new WrappedSetting<bool>(setting);
		}

		public static IReadonlySetting<bool> HasValue<T>(this NotifyList<T> list)
		{
			return new TranslatingSetting<int, bool>(list.CountSetting, count => count > 0);
		}

		public static IReadonlySetting<T> GetSettingOfKey<K, T>(this IReadonlyNotifyDictionary<K, T> src, K key, T valueIfNull = default(T))
		{
			return new NotifyDictionaryValueSetting<K, T>(src, key, valueIfNull);
		}

		public static Thread MainThread;

		public static bool InMainThread()
		{
			return Thread.CurrentThread == MainThread;
		}

		public static Vector3 DirectionToEulerAngles(this Vector3 dir)
		{
			return Quaternion.FromToRotation(Vector3.up, dir).eulerAngles;
		}

		public static float RoundToNearestValue(this float val, float cutoff)
		{
			var remainder = val % cutoff;
			if (remainder < cutoff / 2f)
				return val - remainder;
			else
				return val + (cutoff - remainder);
		}

		public static float RoundToNearestValueWithinDelta(this float currVal, float cutoff, float delta)
		{
			var remainder = currVal % cutoff;

			if (remainder > delta && remainder < cutoff - delta)
				return currVal;

			if (remainder < cutoff / 2f)
				return currVal - remainder;
			else
				return currVal + (cutoff - remainder);
		}

		public static IDisposable ExecInMainThreadRepeating(this IUnityScheduler scheduler, float delay, Action task)
		{
			return new ExecInMainThreadRepeating(delay, task, scheduler);
		}

		public static IDisposable ExecInMainThreadRepeatingEachFrame(this IUnityScheduler scheduler, Action task)
		{
			return new ExecInMainThreadRepeatingEachFrame(task, scheduler);
		}
	}
}
