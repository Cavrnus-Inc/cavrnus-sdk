using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class SolidSurface : HoloComponent, IInteractibleComponent
	{
		[SerializeField] private float surfaceScale = 1f;
		public float SurfaceScale { get { return surfaceScale; } set { surfaceScale = value; } }

		[SerializeField] private bool teleportingAllowed = true;
		public bool TeleportingAllowed { get { return teleportingAllowed; } set { teleportingAllowed = value; } }

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<SolidSurfaceHoloComponent>(parent);
			g.Data.SurfaceScale = SurfaceScale;
			g.Data.TeleportingAllowed = TeleportingAllowed;

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}