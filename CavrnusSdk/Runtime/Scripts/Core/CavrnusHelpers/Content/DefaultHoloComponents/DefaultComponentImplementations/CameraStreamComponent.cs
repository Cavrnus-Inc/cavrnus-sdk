using Collab.Base.Math;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public class CameraStreamComponent : HoloComponent, IInteractibleComponent
	{
		[SerializeField] public int StreamResolutionWidth = 960;
		[SerializeField] public int StreamResolutionHeight = 540;
		[SerializeField] public int StreamFrameRate = 15;
		[SerializeField] public bool UseActiveCameraAspectRatio = false;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo uth)
		{
			var c = t.CreateNewComponent<CameraHoloStreamComponent>(parent);
			c.StreamFrameRate = StreamFrameRate;
			c.StreamResolution = new Int2(StreamResolutionWidth, StreamResolutionHeight);
			c.UseActiveCameraAspectRatio = UseActiveCameraAspectRatio;

			var unityCam = GetComponent<Camera>();
			if (unityCam != null)
			{
				c.FieldOfViewDegrees = unityCam.fieldOfView;
			}
			else
			{
				c.FieldOfViewDegrees = 70f;
			}

			return c.ComponentId;
		}
	}
}
