using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class VisibilityOverrideDisableARComponent : HoloComponent, IInteractibleComponent
	{
		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<VisibilityOverrideDisableARHoloComponent>(parent);
			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}