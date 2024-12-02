using System;
using System.IO;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Base.ProcessSys;
using Collab.Base.ProcessTask;
using Collab.Base.Scheduler;
using Collab.Base.Serialization;
using ExifLib;
using OldMoatGames;
using UnityBase.Import;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityBase.Content
{
	public interface ITextureConverter
	{
		Task<Texture> ConvertTextureFromData(string extension, string name, Stream data, long dataLength, FilterMode filter, IUnityScheduler sched, bool isLinear, bool compress, bool checkRotationMetadata, IProcessFeedback pf = null);
	}

	public static class TextureConvert
	{
		private static ITextureConverter converterInstance = new DefaultTextureConverter();
		public static void SetConverterInstance(ITextureConverter c) { converterInstance = c; }

		public static async Task<Texture> ConvertTextureFromData(string extension, string name, Stream data, long dataLength, FilterMode filter, IUnityScheduler sched, bool isLinear, bool compress, bool checkRotationMetadata, IProcessFeedback pf = null)
		{
			return await converterInstance.ConvertTextureFromData(extension, name, data, dataLength, filter, sched, isLinear, compress, checkRotationMetadata, pf);
		}

		public static unsafe void TextureFromImage2D(ref Texture2D tex, Image2D im)
		{
			if (tex == null ||
			    (tex.width != im.Resolution.x || tex.height != im.Resolution.y))
			{
				if (tex != null)
					UnityEngine.Object.DestroyImmediate(tex);
				tex = new Texture2D(im.Resolution.x, im.Resolution.y, TextureFormat.BGRA32, false, false);
			}

			fixed (void* imagedata = im.ImageData)
			{
				if (im.ImageData != null && im.ImageData.Length != 0)
				{
					tex.LoadRawTextureData(new IntPtr(imagedata), im.ImageData.Length);
					tex.Apply(false, false);
				}
			}
		}

	}

	public class DefaultTextureConverter : ITextureConverter
	{
		public static bool? supportsHalfTex = null;

		public async Task<Texture> ConvertTextureFromData(string extension, string name, Stream data, long dataLength, FilterMode filter, IUnityScheduler sched, bool isLinear, bool compress, bool checkRotationMetadata, IProcessFeedback pf = null)
		{
			if (dataLength > 512 * 1024 * 1024) // Greater than a half giga.
			{
				DebugOutput.Warning("Attempting to load a texture greater than 512MB in size. Texture will not be loaded.");
				return null;
			}

			try
			{
				Texture2D tex = null; // size is overwritten when loading from bytes, so the 4s are irrelevant.
				var ext = extension.ToLowerInvariant();
				if (ext.StartsWith(".png"))
				{
					tex = await DoLoadPngJpg(name, data, dataLength, filter, isLinear, compress, false, sched, pf);
				}
				else if (ext.StartsWith(".jpg") || ext.StartsWith(".jpeg"))
				{
					tex = await DoLoadPngJpg(name, data, dataLength, filter, isLinear, compress, checkRotationMetadata, sched, pf);
				}
				else if (ext.StartsWith(".dds"))
				{
					tex = await sched.ExecInMainThreadAsyncTask(() =>
					{
						var tex2 = DdsTextureLoader.LoadSync(data, name);
						if (tex2 != null)
							tex2.filterMode = filter;
						return tex2;
					});
				}
				else if (ext.StartsWith(".exr"))
				{
					tex = await DoLoadExr(data, dataLength, filter, sched, pf);
					tex.name = name;
				}
				else if (ext.StartsWith(".hdr"))
				{
					tex = await DoLoadHdr(data, dataLength, filter, sched, pf);
					tex.name = name;
				}
				else if (ext.StartsWith(".gif"))
				{
					tex = await DoLoadGif(data, dataLength, name, filter, sched, pf);
					tex.name = name;
				}
				else
				{
					DebugOutput.Warning($"Unknown Texture Format Extension: \"{extension}\".");
				}

				return tex;
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private static async Task<Texture2D> DoLoadGif(Stream data, long dataLen, string name, FilterMode filter, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			await sched.BaseScheduler.ToSchedulerThread();

			MemoryStream buf = data.GetStreamAsMemoryStreamSyncOfLength(dataLen, pf);
			GifDecoder gd = new GifDecoder(true); // Do not dispose this, it will release the memory stream, and the lower AnimatedGifPlayer needs the same stream. Rather than copy, just leave it un-stopped.
			gd.Read(buf);
			buf.Seek(0L, SeekOrigin.Begin);

			GameObject container = new GameObject(name);
			AnimatedGifPlayer gf = container.AddComponent<AnimatedGifPlayer>();
			gf.GifTexture = new Texture2D(gd.GetFrameWidth(), gd.GetFrameHeight(), TextureFormat.RGBA32, false);
			gf.AutoPlay = true;
			gf.Loop = true;
#if !UNITY_WEBGL
			gf.UseThreadedDecoder = true;
#else
			gf.UseThreadedDecoder = false;
#endif
			gf.FileStream = buf;
			gf.Init();

			return gf.GifTexture;
		}

		private static async Task<Texture2D> DoLoadHdr(Stream data, long dataLen, FilterMode filter, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			if (!supportsHalfTex.HasValue)
			{
				await sched.ExecInMainThreadAsyncTask(() =>
				{
					supportsHalfTex = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf);
					return true;
				});
			}

			if (supportsHalfTex.Value)
			{
				return await DoLoadHdrAsHalf(data, dataLen, filter, sched, pf);
			}
			else
			{
				return await DoLoadHdrAsFloat(data, dataLen, filter, sched, pf);
			}
		}

		private static async Task<Texture2D> DoLoadHdrAsFloat(Stream data, long streamLen, FilterMode filter, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			ImageEXRInfo i2 = await sched.ExecInWorkerThreadTask<ImageEXRInfo>(async () =>
			{
				ImageHDRInfo i = null;
				using (pf?.Progress?.Push(new ProgressStep("Load HDR File", $"Load HDR data from stream ({streamLen} bytes), as floats",
					new Span(0f, .25f))))
				{
					i = ImageLoaderHDR.LoadHDRFromStream(data, pf);
				}

				if (i == null)
					return null;

				using (pf?.Progress?.Push(new ProgressStep("Convert HDR File", "Convert RGBE to Floats",
					new Span(.25f, .5f))))
				{
					i2 = ImageLoaderHDR.ConvertHDRRGBEBytesToFloats(i);
					return i2;
				}
			});

			if (i2 == null)
			{
				return null;
			}

			return await sched.ExecInMainThreadAsyncTask(() =>
			{
				using (pf?.Progress?.Push(new ProgressStep("Upload HDR Texture", "Upload Float Texture to GPU",
					new Span(.5f, 1f))))
				{
					return DoUploadFloatTex(i2, filter);
				}
			});
		}

		private static async Task<Texture2D> DoLoadHdrAsHalf(Stream data, long dataLen, FilterMode filter, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			ImageEXRHalfsInfo i2 = await sched.ExecInWorkerThreadTask<ImageEXRHalfsInfo>(async () =>
			{
				ImageHDRInfo i = null;
				using (pf?.Progress?.Push(new ProgressStep("Load HDR File", $"Load HDR data from stream ({dataLen} bytes), as halfs",
					new Span(0f, .25f))))
				{
					i = ImageLoaderHDR.LoadHDRFromStream(data, pf);
				}

				if (i == null)
					return null;

				using (pf?.Progress?.Push(new ProgressStep("Convert HDR File", "Convert RGBE to Halfs",
					new Span(.25f, .5f))))
				{
					i2 = ImageLoaderHDR.ConvertHDRRGBEBytesToHalfs(i);
					return i2;
				}
			});

			if (i2 == null)
			{
				return null;
			}

			return await sched.ExecInMainThreadAsyncTask(() =>
			{
				using (pf?.Progress?.Push(new ProgressStep("Upload HDR Texture", "Upload Halfs Texture to GPU",
					new Span(.5f, 1f))))
				{
					return DoUploadHalfsTex(i2, filter);
				}
			});
		}
		private static async Task<Texture2D> DoLoadExr(Stream data, long dataLen, FilterMode filter, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			var i = await sched.ExecInWorkerThreadTask(async () =>
			{
				// unfortunately the EXR loader seeks. We need a raw buf
				using (var buf = await data.GetStreamAsMemoryStreamAsyncOfLength(dataLen, pf))
				{
					return await ImageLoaderEXR.LoadEXRFromStream(buf, pf);
				}
			});

			return await sched.ExecInMainThreadAsyncTask(() =>
			{
				using (pf?.Progress?.Push(new ProgressStep("Upload EXR Texture", "Upload Float Texture to GPU",
					new Span(.5f, 1f))))
				{
					return DoUploadFloatTex(i, filter);
				}
			});
		}

		private static unsafe Texture2D DoUploadFloatTex(ImageEXRInfo i2, FilterMode filter)
		{
			var tex = new Texture2D(i2.resolution.x, i2.resolution.y, TextureFormat.RGBAFloat, false, true);
			fixed (void* rgbaraw = i2.rgbafloats)
			{
				tex.LoadRawTextureData(new IntPtr(rgbaraw), i2.rgbafloats.Length * sizeof(float));
				tex.filterMode = filter;
				tex.Apply(false, true);
			}

			return tex;
		}

		private static unsafe Texture2D DoUploadHalfsTex(ImageEXRHalfsInfo i2, FilterMode filter)
		{
			var tex = new Texture2D(i2.resolution.x, i2.resolution.y, TextureFormat.RGBAHalf, false, true);
			fixed (void* rgbaraw = i2.rgbahalfs)
			{
				tex.LoadRawTextureData(new IntPtr(rgbaraw), i2.rgbahalfs.Length * sizeof(byte) * 2);
				tex.filterMode = filter;
				tex.Apply(false, true);
			}

			return tex;
		}
		private static async Task<Texture2D> DoLoadPngJpg(string name, Stream data, long dataLen, FilterMode filter, bool isLinear, bool compress, bool checkRotationMetadata, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			var memBuf = await data.GetStreamAsMemoryStreamAsyncOfLength(dataLen, pf);

			return await sched.ExecInMainThreadAsyncTask(() =>
			{
				try
				{
					Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32,
						filter == FilterMode.Trilinear, isLinear);

					tex.filterMode = filter;
					tex.name = name;
					tex.LoadImage(memBuf.GetBuffer());

					if (checkRotationMetadata)
					{
						var exifreader = new ExifReader(memBuf);
						ExifLib.JpegInfo jpi = exifreader.info;
						SetOrientation(ref tex, jpi.Orientation);
					}

					if (compress && Mathf.IsPowerOfTwo(tex.width) && Mathf.IsPowerOfTwo(tex.height) &&
					    tex.width >= 32 && tex.height >= 32)
						tex.Compress(true);
					tex.Apply(true, true);

					return tex;
				}
				catch (Exception aaaaaa)
				{
					DebugOutput.Error($"AAAAAAAAAA: {aaaaaa}");
					return null;
				}
			});
		}

		public static Color[] CreateColorArrayFromExr(ImageEXRInfo i)
		{
			Color[] cs = new Color[i.resolution.x * i.resolution.y];
			for (int y = i.resolution.y - 1; y >= 0; y--)
			{
				for (int x = 0; x < i.resolution.x; x++)
				{
					int outInd = (i.resolution.y - y - 1) * i.resolution.x + x;
					int inInd = y * i.resolution.x + x;
					cs[outInd] = new Color(i.rgbafloats[inInd * 3 + 0], i.rgbafloats[inInd * 3 + 1], i.rgbafloats[inInd * 3 + 2]);
				}
			}
			return cs;
		}
		


		// This texture re-orientation method is slow but acceptable for now.
		private static bool SetOrientation(ref Texture2D texture, ExifLib.ExifOrientation orientation)
		{
			switch (orientation)
			{
				case ExifLib.ExifOrientation.TopLeft:
				case ExifLib.ExifOrientation.Undefined:
				default:
					return false;
				case ExifLib.ExifOrientation.BottomRight:
					texture = RotateTexture(texture, true);
					texture = RotateTexture(texture, true);
					break;
				case ExifLib.ExifOrientation.TopRight:
					texture = RotateTexture(texture, true);
					break;
				case ExifLib.ExifOrientation.BottomLeft:
					texture = RotateTexture(texture, false);
					break;
			}

			return true;
		}

		// This texture re-orientation method is slow but acceptable for now.
		private static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
		{
			if (!originalTexture.isReadable)
				return originalTexture;

			Color32[] original = originalTexture.GetPixels32();
			Color32[] rotated = new Color32[original.Length];
			int w = originalTexture.width;
			int h = originalTexture.height;

			int iRotated, iOriginal;

			for (int j = 0; j < h; ++j)
			{
				for (int i = 0; i < w; ++i)
				{
					iRotated = (i + 1) * h - j - 1;
					iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
					rotated[iRotated] = original[iOriginal];
				}
			}

			Texture.Destroy(originalTexture);

			Texture2D rotatedTexture = new Texture2D(h, w);
			rotatedTexture.SetPixels32(rotated);
			return rotatedTexture;
		}
	}
}