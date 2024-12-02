using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Prop.ContainerContext;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.LinkProp;
using UnityEngine;

namespace UnityBase.PropRenderers
{
	public class UnityTextureImplementation : ITexturePropertyRenderer
	{
		public Texture tex;

		public UnityTextureImplementation(Texture tex)
		{
			this.tex = tex;
		}

		public virtual void SwapTexture(Texture tex)
		{
			var filt = this.tex?.filterMode ?? FilterMode.Bilinear;
			var wrapu = this.tex?.wrapModeU ?? TextureWrapMode.Repeat;
			var wrapv = this.tex?.wrapModeV ?? TextureWrapMode.Repeat;
			var name = this.tex?.name ?? "";

			this.tex = tex;

			if (this.tex != null)
			{
				this.tex.name = name;
				this.tex.filterMode = filt;
				this.tex.wrapModeU = wrapu;
				this.tex.wrapModeV = wrapv;
			}
		}

		public void SetName(string name)
		{
			if (tex == null)
				return;
			this.tex.name = name;
		}

		public void SetFiltering(TexturePropertyDefs.Textures_Filtering_Enum filtering)
		{
			if (tex == null)
				return;
			switch (filtering)
			{
				case TexturePropertyDefs.Textures_Filtering_Enum.point:
					this.tex.filterMode = FilterMode.Point;
					break;
				case TexturePropertyDefs.Textures_Filtering_Enum.bilinear:
					this.tex.filterMode = FilterMode.Bilinear;
					break;
				case TexturePropertyDefs.Textures_Filtering_Enum.trilinear:
				default:
					this.tex.filterMode = FilterMode.Trilinear;
					break;
			}
		}

		public void SetWrapU(TexturePropertyDefs.Textures_Wrap_Enum wrap)
		{
			if (tex == null)
				return;
			switch (wrap)
			{
				case TexturePropertyDefs.Textures_Wrap_Enum.clamp:
					this.tex.wrapModeU = TextureWrapMode.Clamp;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.repeat:
					this.tex.wrapModeU = TextureWrapMode.Repeat;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirror:
					this.tex.wrapModeU = TextureWrapMode.Mirror;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirroronce:
					this.tex.wrapModeU = TextureWrapMode.MirrorOnce;
					break;
			}
		}

		public void SetWrapV(TexturePropertyDefs.Textures_Wrap_Enum wrap)
		{
			if (tex == null)
				return;
			switch (wrap)
			{
				case TexturePropertyDefs.Textures_Wrap_Enum.clamp:
					this.tex.wrapModeV = TextureWrapMode.Clamp;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.repeat:
					this.tex.wrapModeV = TextureWrapMode.Repeat;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirror:
					this.tex.wrapModeV = TextureWrapMode.Mirror;
					break;
				case TexturePropertyDefs.Textures_Wrap_Enum.mirroronce:
					this.tex.wrapModeV = TextureWrapMode.MirrorOnce;
					break;
			}
		}
	}
}
