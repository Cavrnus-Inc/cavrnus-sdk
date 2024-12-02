using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class EyeController : HoloComponent
	{
		public bool UseAverageEyePos { get { return useAverageEyePos; } set { useAverageEyePos = value; } }
		[SerializeField] private bool useAverageEyePos = false;
		public float CutoffAngle { get { return cutoffAngle; } set { cutoffAngle = value; } }
		[SerializeField] private float cutoffAngle = 20f;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var g = t.CreateNewComponent<EyeBehaviourHoloComponent>(parent);

			g.Data.UseAverageEyePos = UseAverageEyePos;
			g.Data.CutoffAngle = CutoffAngle;

			g.Tags = Tags;

			return g.ComponentId;
		}
	}
}