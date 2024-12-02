using Collab.Proxy.Prop.JournalInterop;
using UnityEngine;

namespace UnityBase.PropRenderers
{

	public class UnityCubeTextureImplementation : ICubeTexturePropertyRenderer
	{
		public Cubemap tex;

		public UnityCubeTextureImplementation(Cubemap tex)
		{
			this.tex = tex;
		}

		public virtual void SwapTexture(Cubemap tex)
		{
			var filt = this.tex.filterMode;
			var name = this.tex.name;

			this.tex = tex;

			this.tex.name = name;
			this.tex.filterMode = filt;
		}

		public void SetName(string name)
		{
			this.tex.name = name;
		}

		public void SetFiltering(TexturePropertyDefs.Textures_Filtering_Enum filtering)
		{
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
	}
}