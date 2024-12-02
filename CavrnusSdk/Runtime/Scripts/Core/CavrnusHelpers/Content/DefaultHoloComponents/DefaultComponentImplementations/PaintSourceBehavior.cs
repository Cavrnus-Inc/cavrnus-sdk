using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class PaintSourceBehavior : HoloComponent, IInteractibleComponent
	{
		public string PaintId { get { return paintId; } set { paintId = value; } }
		[SerializeField] private string paintId;
		public string PaintCategory { get { return paintCategory; } set { paintCategory = value; } }
		[SerializeField] private string paintCategory;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<PaintSourceBehaviourHoloComponent>(parent);

			g.Data.PaintId = PaintId;
			g.Data.PaintCategory = PaintCategory;

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}