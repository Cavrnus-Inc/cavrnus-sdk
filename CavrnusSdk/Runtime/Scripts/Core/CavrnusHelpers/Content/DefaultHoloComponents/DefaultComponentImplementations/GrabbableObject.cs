using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class GrabbableObject : HoloComponent, IInteractibleComponent
	{
		[SerializeField] private GrabbableData.MovementTypeEnum moveType;
		public GrabbableData.MovementTypeEnum MoveType { get { return moveType; } set { moveType = value; } }

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<GrabbableBehaviourHoloComponent>(parent);
			g.Data.MoveType = MoveType;

			g.Tags = Tags;
			return g.ComponentId;
		}
	}
}
