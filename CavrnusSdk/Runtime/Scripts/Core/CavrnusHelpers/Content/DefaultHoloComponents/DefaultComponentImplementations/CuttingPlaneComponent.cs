using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using Collab.Base.Math;
using UnityBase;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class CuttingPlaneComponent : HoloComponent
	{
		[SerializeField] private Vector3 size;
		[SerializeField] private List<string> layersToSkip;

		public List<string> TagsToSkip { get { return layersToSkip; } set { layersToSkip = value; } }
		public Vector3 CuttingPlaneSize { get { return size; } set { size = value; } }

		public Color borderColor = new Color(.8f, .4f, 0f, 1f);
		public float borderSize = 0.0025f;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<CuttingPlaneHoloStreamComponent>(parent);
			g.Size = CuttingPlaneSize.ToFloat3();
			g.TagsToSkip = TagsToSkip;
			g.BorderColor = borderColor.ToColor4();
			g.BorderSize = borderSize;
			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}