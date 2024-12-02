using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityBase
{
	public class TextureWithUVs
	{
		public Texture Texture;
		public Rect UVRect;

		public TextureWithUVs(Texture texture, Rect uvRect)
		{
			Texture = texture;
			UVRect = uvRect;
		}

		public TextureWithUVs(Texture texture)
		{
			Texture = texture;
			UVRect = new Rect(0, 0, 1, 1);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is TextureWithUVs ot))
				return false;
			if (!ot.UVRect.Equals(UVRect))
				return false;
			return ReferenceEquals(this.Texture, ot.Texture);
		}

		public override int GetHashCode()
		{
			return Texture != null ? Texture.GetHashCode() : 0;
		}
	}
}
