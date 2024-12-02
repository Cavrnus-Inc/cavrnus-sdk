using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Math;
using Collab.Holo;
using Collab.Proxy.Prop.JournalInterop;
using UnityEngine;

namespace UnityBase
{
	public class UnityLightImplementation : ILightPropertyRenderer
	{
		private Light light;
		public UnityLightImplementation(Light light)
		{
			this.light = light;
		}

		public void SetLightType(LightHoloComponent.LightTypeEnum lightType)
		{
			if (lightType == LightHoloComponent.LightTypeEnum.Directional)
				light.type = LightType.Directional;
			else if (lightType == LightHoloComponent.LightTypeEnum.Spot)
				light.type = LightType.Spot;
			else
				light.type = LightType.Point;
		}

		public void SetShadowMode(LightHoloComponent.ShadowMode shadowMode)
		{
			if (shadowMode == LightHoloComponent.ShadowMode.Hard)
				light.shadows = LightShadows.Hard;
			else if (shadowMode == LightHoloComponent.ShadowMode.Soft)
				light.shadows = LightShadows.Soft;
			else
				light.shadows = LightShadows.None;
		}

		public void SetIntensity(double intensity)
		{
			light.intensity = (float)intensity;
		}

		public void SetIndirectMultiplier(double mult)
		{
			light.bounceIntensity = (float)mult;
		}

		public void SetShadowStrength(double strength)
		{
			light.shadowStrength = (float)strength;
		}

		public void SetEnabled(bool vis)
		{
			light.enabled = vis;
		}

		public void SetLightRange(double range)
		{
			light.range = (float)range;
		}

		public void SetSpotAngleInnerDegrees(double degrees)
		{
			light.innerSpotAngle = (float)degrees;
		}

		public void SetSpotAngleOuterDegrees(double degrees)
		{
			light.spotAngle = (float)degrees;
		}

		public void SetColor(Color4F color)
		{
			light.color = color.ToColor();
		}
	}
}
