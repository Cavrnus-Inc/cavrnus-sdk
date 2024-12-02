using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class StreamSurfaceComponent : HoloComponent
	{
		public UserStreamSurfaceData data;
		public Material material;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<UserStreamSurfaceHoloComponent>(parent);
			g.Data = (UserStreamSurfaceData)data?.Clone() ?? new UserStreamSurfaceData();
			g.Data.UseSpecificMaterial = uth.ConvertMaterial(this.material, t, null, null);
			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}