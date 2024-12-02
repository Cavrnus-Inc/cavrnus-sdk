using System;
using System.Collections.Generic;
using Collab.Holo;
using Collab.Base.Core;
using Collab.Base.Math;
using UnityEngine;

namespace UnityBase.Content
{
	[Serializable]
	public class LightmapRendererInfo
	{
		public MeshRenderer renderer;
		public int lightmapIndex;
		public Vector4 lightmapOffsetScale;
	}
}
