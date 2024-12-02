using System.Collections;
using System.Collections.Generic;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class HoloArTrackerBehavior : HoloComponent, IInteractibleComponent
	{
		[SerializeField] private string trackerName;
		public string TrackerName { get { return trackerName; } set { trackerName = value; } }

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<ARTrackerBehaviourHoloComponent>(parent);
			g.TrackerName = trackerName;

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}