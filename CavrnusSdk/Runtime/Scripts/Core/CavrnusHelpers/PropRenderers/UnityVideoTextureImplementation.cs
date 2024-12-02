using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ContainerContext;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.LinkProp;
using UnityBase.LiveUnityManagement;
using UnityEngine;
using UnityEngine.Video;

namespace UnityBase.PropRenderers
{
	public class UnityVideoTextureImplementation : IVideoTexturePropertyRenderer, IContextProvidingPropertyRenderer
	{
		public VideoPlayer vp;
		private Setting<Texture> vidTex = new Setting<Texture>(null);
		public IReadonlySetting<Texture> Texture => vidTex;
		private Setting<double> vidLength = new Setting<double>(1.0);
		public IReadonlySetting<double> PlaybackLength => vidLength;

		private TexturePropertyDefs.Textures_Filtering_Enum filtering = TexturePropertyDefs.Textures_Filtering_Enum.point;
		private TexturePropertyDefs.Textures_Wrap_Enum wrapu = TexturePropertyDefs.Textures_Wrap_Enum.clamp;
		private TexturePropertyDefs.Textures_Wrap_Enum wrapv = TexturePropertyDefs.Textures_Wrap_Enum.clamp;

		// Try and remove this to remove duped memory.
		private TextureAssetHoloStreamComponent c;

		private PropertySetManager container;

		public UnityVideoTextureImplementation(VideoPlayer vp, TextureAssetHoloStreamComponent c)
		{
			this.vp = vp;
			this.c = c;

			if (!vp.isPrepared)
			{
				vp.Prepare();
				vp.prepareCompleted += VpOnprepareCompleted;
			}
			else
			{
				VpOnprepareCompleted(vp);
			}
		}

		public void SwapVideo(VideoPlayer vp)
		{
			this.vp = vp;

			if (!vp.isPrepared)
			{
				vp.Prepare();
				vp.prepareCompleted += VpOnprepareCompleted;
			}
			else
			{
				VpOnprepareCompleted(vp);
			}
		}

		private void VpOnprepareCompleted(VideoPlayer source)
		{
			vidLength.Value = source.length;
			
			vp.prepareCompleted -= VpOnprepareCompleted;
			vidTex.Value = source.texture;
			if (vidTex.Value != null)
			{
				SetName(this.vp.name);
				SetFiltering(this.filtering);
				SetWrapU(this.wrapu);
				SetWrapV(this.wrapv);
			}
		}

		public void SetName(string name)
		{
			this.vp.name = name;
			if (this.vidTex.Value != null)
				this.vidTex.Value.name = name;
		}

		public void SetFiltering(TexturePropertyDefs.Textures_Filtering_Enum filtering)
		{
			this.filtering = filtering;
			if (this.vidTex.Value == null)
				return;

			switch (filtering)
			{
				case TexturePropertyDefs.Textures_Filtering_Enum.point:
					this.vidTex.Value.filterMode = FilterMode.Point;
					break;
				case TexturePropertyDefs.Textures_Filtering_Enum.bilinear:
					this.vidTex.Value.filterMode = FilterMode.Bilinear;
					break;
				case TexturePropertyDefs.Textures_Filtering_Enum.trilinear:
				default:
					this.vidTex.Value.filterMode = FilterMode.Trilinear;
					break;
			}
		}

		public void SetWrapU(TexturePropertyDefs.Textures_Wrap_Enum wrap)
		{
			this.wrapu = wrap;
			if (this.vidTex.Value == null)
				return;

			switch (wrap)
			{
				case TexturePropertyDefs.Textures_Wrap_Enum.clamp:
					this.vidTex.Value.wrapModeU = TextureWrapMode.Clamp;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.repeat:
					this.vidTex.Value.wrapModeU = TextureWrapMode.Repeat;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirror:
					this.vidTex.Value.wrapModeU = TextureWrapMode.Mirror;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirroronce:
					this.vidTex.Value.wrapModeU = TextureWrapMode.MirrorOnce;
					break;
			}
		}

		public void SetWrapV(TexturePropertyDefs.Textures_Wrap_Enum wrap)
		{
			this.wrapv = wrap;
			if (this.vidTex.Value == null)
				return;

			switch (wrap)
			{
				case TexturePropertyDefs.Textures_Wrap_Enum.clamp:
					this.vidTex.Value.wrapModeV = TextureWrapMode.Clamp;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.repeat:
					this.vidTex.Value.wrapModeV = TextureWrapMode.Repeat;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirror:
					this.vidTex.Value.wrapModeV = TextureWrapMode.Mirror;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirroronce:
					this.vidTex.Value.wrapModeV = TextureWrapMode.MirrorOnce;
					break;
			}
		}

		public IDisposable SetupContext(PropertySetManager container)
		{
			this.container = container;

			var d1 = container.AddContext(new HoloVideoTextureContext(this, new TextureContentType() { Usage = c.TextureCategory.ToTextureContentType() }, c));
			var d2 = container.AddContext(new HoloVideoTextureThumbnailContext(this));
			return d1.AlsoDispose(d2);
		}

		public void SetVideoVolume(double v)
		{
			vp.SetDirectAudioVolume(0, Mathf.Clamp((float)v, 0, 10));
		}

		public void SetLooping(bool looping)
		{
			vp.isLooping = looping;
		}

		private double? lastUpdateTime = null;
		public void UpdatePlaybackState(bool playing, double curTime, double speed)
		{
			if (!vp.playbackSpeed.AlmostEquals((float)speed))
				vp.playbackSpeed = (float)speed;

			if (vp.isPlaying != playing)
			{
				lastUpdateTime = Time.time;
				if (playing)
					vp.Play();
				else
					vp.Pause();
			}

			if (Math.Abs(curTime - vp.time) > .5 && lastUpdateTime != null && Time.time - lastUpdateTime > .5)
			{
				lastUpdateTime = Time.time;
				vp.time = curTime;
			}
		}
	}
}
