using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using UnityBase;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class CustomObjectFocusPositionComponent : HoloComponent
	{
		public Vector3 RelativeCameraPosition { get { return relativeCameraPosition; } set { relativeCameraPosition = value; } }
		[SerializeField] private Vector3 relativeCameraPosition;

		public Vector3 ReativeCameraLookPosition { get { return reativeCameraLookPosition; } set { reativeCameraLookPosition = value; } }
		[SerializeField] private Vector3 reativeCameraLookPosition;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo conversions)
		{
			var g = t.CreateNewComponent<FocusLocusHoloComponent>(parent);

			g.Data.RelativeCameraPos = ReativeCameraLookPosition.ToFloat3();
			g.Data.RelativeFocusPosition = RelativeCameraPosition.ToFloat3();

			g.Tags = Tags;

			return g.ComponentId;
		}
	}
}