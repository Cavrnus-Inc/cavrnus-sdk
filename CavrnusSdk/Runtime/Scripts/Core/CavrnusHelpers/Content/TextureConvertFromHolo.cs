using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Scheduler;
using Collab.Base.Serialization;
using Collab.Holo;
using Collab.Proxy;
using UnityEngine;
using UnityEngine.Video;

namespace UnityBase.Content
{
	public static class TextureConvertFromHolo
	{
		public static async Task<Texture> ConvertTexture(TextureAssetHoloStreamComponent t, IUnityScheduler sched, bool checkRotationMetadata, IProcessFeedback pf = null)
		{
			if (t.ImageData == null || t.ImageData.Length <= 1)
				return null; // no data!

			bool isLinear = Image2D.GetIsLinear(t.TextureCategory);
			bool shouldCompress = Image2D.GetCanCompress(t.TextureCategory);

			return await TextureConvert.ConvertTextureFromData(
				ConvertImageFormatFromHolo(t.ImageFormat), t.ImageFileName,
				new MemoryStream(t.ImageData, 0, t.ImageData.Length, false, true),
				t.ImageData.LongLength, ConvertFilterModeFromHolo(t.TextureFilter), sched, isLinear, shouldCompress, checkRotationMetadata, pf);
		}

		private static string ConvertImageFormatFromHolo(TextureAssetHoloStreamComponent.ImageHoloPartFormatType f)
		{
			switch (f)
			{
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Exr:
					return ".exr";
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Dds:
					return ".dds";
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Png:
					return ".png";
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Hdr:
					return ".hdr";
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Gif:
					return ".gif";
				case TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Jpg:
				default:
					return ".jpg";
			}
		}

		private static FilterMode ConvertFilterModeFromHolo(TextureAssetHoloStreamComponent.ImageFilteringEnum f)
		{
			switch (f)
			{
				case TextureAssetHoloStreamComponent.ImageFilteringEnum.Trilinear:
					return FilterMode.Trilinear;
				case TextureAssetHoloStreamComponent.ImageFilteringEnum.Bilinear:
					return FilterMode.Bilinear;
				case TextureAssetHoloStreamComponent.ImageFilteringEnum.Point:
				default:
					return FilterMode.Point;
			}
		}

		public static async Task<Cubemap> ConvertToCubemapTexture(CubeTextureAssetHoloStreamComponent t)
		{
			Cubemap newCubemap = null;
			switch (t.ImageFormat)
			{
				case CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Png:
				{
					Texture2D tex = new Texture2D(4, 4);
					tex.LoadImage(t.PositiveXPixels);
					if (Mathf.IsPowerOfTwo(tex.width) && Mathf.IsPowerOfTwo(tex.height) && tex.width >= 32 && tex.height >= 32)
						tex.Compress(true);
					tex.Apply(true, true);

					newCubemap = new Cubemap(tex.width, TextureFormat.RGBA32, true);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveXPixels), CubemapFace.PositiveX);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeXPixels), CubemapFace.NegativeX);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveYPixels), CubemapFace.PositiveY);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeYPixels), CubemapFace.NegativeY);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveZPixels), CubemapFace.PositiveZ);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeZPixels), CubemapFace.NegativeZ);
					newCubemap.Apply(true, true);
				}
					break;

				case CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Jpg:
				{
					Texture2D tex = new Texture2D(4, 4);
					tex.LoadImage(t.PositiveXPixels);
					if (Mathf.IsPowerOfTwo(tex.width) && Mathf.IsPowerOfTwo(tex.height) && tex.width >= 32 && tex.height >= 32)
						tex.Compress(true);
					tex.Apply(true, true);

					newCubemap = new Cubemap(tex.width, TextureFormat.RGBA32, true);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveXPixels), CubemapFace.PositiveX);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeXPixels), CubemapFace.NegativeX);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveYPixels), CubemapFace.PositiveY);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeYPixels), CubemapFace.NegativeY);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.PositiveZPixels), CubemapFace.PositiveZ);
					newCubemap.SetPixels(CreateColorArrayFromByteArray(t.NegativeZPixels), CubemapFace.NegativeZ);
					newCubemap.Apply(true, true);
				}
					break;

				case CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Dds:
					DebugOutput.Error("Dds not supported for cubemaps!");
					break;

				case CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Exr:
				{
					var posXExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.PositiveXPixels, 0, t.PositiveXPixels.Length, false, true));
					var negXExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.NegativeXPixels, 0, t.NegativeXPixels.Length, false, true));
					var posYExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.PositiveYPixels, 0, t.PositiveYPixels.Length, false, true));
					var negYExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.NegativeYPixels, 0, t.NegativeYPixels.Length, false, true));
					var posZExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.PositiveZPixels, 0, t.PositiveZPixels.Length, false, true));
					var negZExr = await ImageLoaderEXR.LoadEXRFromStream(new MemoryStream(t.NegativeZPixels, 0, t.NegativeZPixels.Length, false, true));

					newCubemap = new Cubemap(posXExr.resolution.x, TextureFormat.RGBAFloat, true); //size is temp value
						
					newCubemap.SetPixelData(posXExr.rgbafloats, 0, CubemapFace.PositiveX);
					newCubemap.SetPixelData(negXExr.rgbafloats, 0, CubemapFace.NegativeX);
					newCubemap.SetPixelData(posYExr.rgbafloats, 0, CubemapFace.PositiveY);
					newCubemap.SetPixelData(negYExr.rgbafloats, 0, CubemapFace.NegativeY);
					newCubemap.SetPixelData(posZExr.rgbafloats, 0, CubemapFace.PositiveZ);
					newCubemap.SetPixelData(negZExr.rgbafloats, 0, CubemapFace.NegativeZ);
					/*newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(posXExr), CubemapFace.PositiveX);
					newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(negXExr), CubemapFace.NegativeX);
					newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(posYExr), CubemapFace.PositiveY);
					newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(negYExr), CubemapFace.NegativeY);
					newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(posZExr), CubemapFace.PositiveZ);
					newCubemap.SetPixels(TextureConvert.CreateColorArrayFromExr(negZExr), CubemapFace.NegativeZ);*/
					newCubemap.Apply(true, true);
				}
					break;
			}

			return newCubemap;
		}

		public static Color[] CreateColorArrayFromByteArray(byte[] byteArray)
		{
			Texture2D tex = new Texture2D(4, 4);
			tex.LoadImage(byteArray);
			if (Mathf.IsPowerOfTwo(tex.width) && Mathf.IsPowerOfTwo(tex.height) && tex.width >= 32 && tex.height >= 32)
				tex.Compress(true);
			tex.Apply(true);
			return tex.GetPixels();
		}

		public static CubeTextureAssetHoloStreamComponent ConvertCubeToHolo(HoloRoot t, Cubemap cubeTex)
		{
			var t3d = cubeTex;
			if (t3d == null)
				return null;

			bool hdr = false;
			bool alpha = true;
			RenderTextureFormat tmpFmt = RenderTextureFormat.ARGB32;
			TextureFormat writeFmt = TextureFormat.RGBA32;
			switch (t3d.format)
			{
				case TextureFormat.RGB9e5Float:
				case TextureFormat.RGBAFloat:
				case TextureFormat.RGBAHalf:
				case TextureFormat.RFloat:
				case TextureFormat.RGFloat:
				case TextureFormat.RGHalf:
				case TextureFormat.BC6H:
					tmpFmt = RenderTextureFormat.ARGBHalf;
					writeFmt = TextureFormat.RGBAHalf;
					hdr = true;
					break;
				case TextureFormat.RGB24:
				case TextureFormat.R16:
				case TextureFormat.RGB565:
				case TextureFormat.R8:
					alpha = false;
					writeFmt = TextureFormat.RGB24;
					break;
			}

			Color[] posX = t3d.GetPixels(CubemapFace.PositiveX);
			Color[] negX = t3d.GetPixels(CubemapFace.NegativeX);
			Color[] posY = t3d.GetPixels(CubemapFace.PositiveY);
			Color[] negY = t3d.GetPixels(CubemapFace.NegativeY);
			Color[] posZ = t3d.GetPixels(CubemapFace.PositiveZ);
			Color[] negZ = t3d.GetPixels(CubemapFace.NegativeZ);

			Texture2D PositiveX2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				PositiveX2d.SetPixels(posX);
			}
			catch (UnityException)
			{
				Debug.Log("Exception!");
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				PositiveX2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				PositiveX2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			Texture2D NegativeX2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				NegativeX2d.SetPixels(negX);
			}
			catch (UnityException)
			{
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				NegativeX2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				NegativeX2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			Texture2D PositiveY2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				PositiveY2d.SetPixels(posY);
			}
			catch (UnityException)
			{
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				PositiveY2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				PositiveY2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			Texture2D NegativeY2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				NegativeY2d.SetPixels(negY);
			}
			catch (UnityException)
			{
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				NegativeY2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				NegativeY2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			Texture2D PositiveZ2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				PositiveZ2d.SetPixels(posZ);
			}
			catch (UnityException)
			{
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				PositiveZ2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				PositiveZ2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			Texture2D NegativeZ2d = new Texture2D(t3d.width, t3d.height, writeFmt, false);
			try
			{
				NegativeZ2d.SetPixels(negZ);
			}
			catch (UnityException)
			{
				var tmp = RenderTexture.GetTemporary(t3d.width, t3d.height, 0, tmpFmt);
				Graphics.Blit(t3d, tmp);

				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = tmp;

				NegativeZ2d.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0, false);
				NegativeZ2d.Apply();

				RenderTexture.active = prev;
				RenderTexture.ReleaseTemporary(tmp);
			}

			var tc = t.CreateNewRootComponent<CubeTextureAssetHoloStreamComponent>();

			if (hdr)
			{
				byte[] fullSizeExrPosX = PositiveX2d.EncodeToEXR(Texture2D.EXRFlags.None);
				byte[] fullSizeExrNegX = NegativeX2d.EncodeToEXR(Texture2D.EXRFlags.None);
				byte[] fullSizeExrPosY = PositiveY2d.EncodeToEXR(Texture2D.EXRFlags.None);
				byte[] fullSizeExrNegY = NegativeY2d.EncodeToEXR(Texture2D.EXRFlags.None);
				byte[] fullSizeExrPosZ = PositiveZ2d.EncodeToEXR(Texture2D.EXRFlags.None);
				byte[] fullSizeExrNegZ = NegativeZ2d.EncodeToEXR(Texture2D.EXRFlags.None);
				tc.ImageFormat = CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Exr;
				tc.PositiveXPixels = fullSizeExrPosX;
				tc.NegativeXPixels = fullSizeExrNegX;
				tc.PositiveYPixels = fullSizeExrPosY;
				tc.NegativeYPixels = fullSizeExrNegY;
				tc.PositiveZPixels = fullSizeExrPosZ;
				tc.NegativeZPixels = fullSizeExrNegZ;
			}
			else if (!alpha)
			{
				byte[] fullSizeJpgPosX = PositiveX2d.EncodeToJPG(100);
				byte[] fullSizeJpgNegX = NegativeX2d.EncodeToJPG(100);
				byte[] fullSizeJpgPosY = PositiveY2d.EncodeToJPG(100);
				byte[] fullSizeJpgNegY = NegativeY2d.EncodeToJPG(100);
				byte[] fullSizeJpgPosZ = PositiveZ2d.EncodeToJPG(100);
				byte[] fullSizeJpgNegZ = NegativeZ2d.EncodeToJPG(100);
				tc.ImageFormat = CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Jpg;
				tc.PositiveXPixels = fullSizeJpgPosX;
				tc.NegativeXPixels = fullSizeJpgNegX;
				tc.PositiveYPixels = fullSizeJpgPosY;
				tc.NegativeYPixels = fullSizeJpgNegY;
				tc.PositiveZPixels = fullSizeJpgPosZ;
				tc.NegativeZPixels = fullSizeJpgNegZ;
			}
			else
			{
				byte[] fullSizePngPosX = PositiveX2d.EncodeToPNG();
				byte[] fullSizePngNegX = NegativeX2d.EncodeToPNG();
				byte[] fullSizePngPosY = PositiveY2d.EncodeToPNG();
				byte[] fullSizePngNegY = NegativeY2d.EncodeToPNG();
				byte[] fullSizePngPosZ = PositiveZ2d.EncodeToPNG();
				byte[] fullSizePngNegZ = NegativeZ2d.EncodeToPNG();
				tc.ImageFormat = CubeTextureAssetHoloStreamComponent.ImageHoloPartFormatType.Png;
				tc.PositiveXPixels = fullSizePngPosX;
				tc.NegativeXPixels = fullSizePngNegX;
				tc.PositiveYPixels = fullSizePngPosY;
				tc.NegativeYPixels = fullSizePngNegY;
				tc.PositiveZPixels = fullSizePngPosZ;
				tc.NegativeZPixels = fullSizePngNegZ;
			}

			return tc;
		}

		public static async Task<VideoPlayer> ConvertVideoTexture(TextureAssetHoloStreamComponent t, GameObject parent, IUnityScheduler sched, IProcessFeedback pf = null)
		{
			if (t.ImageData == null || t.ImageData.Length <= 1)
				return null; // no data!

			// Store in temporary file, add retention behaviour.
			string ext = ".mp4";
			if (t.ImageFormat == TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Webm)
				ext = ".webm";
			else if (t.ImageFormat == TextureAssetHoloStreamComponent.ImageHoloPartFormatType.Mp4)
				ext = ".mp4";
			else
				throw new NotImplementedException($"Unexpected image format in ConvertVideoTexture: '{t.ImageFormat}'.");

			string tmpPath = CollabPaths.NewTemporaryFilePath(ext);
			await new MemoryStream(t.ImageData).StreamToFileAsync(tmpPath, pf);
			
			var tfradb = parent.AddComponent<TemporaryFileRetentionAndDisposalBehavior>();
			tfradb.SetFilePath(tmpPath);

			// TODO VIDTEX
			VideoPlayer vp = parent.AddComponent<VideoPlayer>();
			vp.url = $"file:///{tmpPath}";
			vp.isLooping = t.VideoLoop;
			vp.SetDirectAudioVolume(0, t.VideoAudioVolume);
			
			return vp;
		}
	}
}
