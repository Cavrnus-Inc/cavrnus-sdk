using System;
using Collab.Base.Collections;
using Collab.Base.Math;
using Collab.Base.ProcessTask;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.LinkProp;
using UnityBase;
using UnityEngine;

public class ManagedRoom : IDisposedElement, IDisposable
{
	private Material mat;
	private PropertySetManager roomPropsContainer;

	public void EstablishRoomProperties(PropertySetManager container)
	{
		this.roomPropsContainer = container;

		mat = new Material(Shader.Find("Cavrnus/Skybox Fog"));
		mat.SetTexture("_Tex", RenderSettings.skybox.GetTexture("_Tex"));
		RenderSettings.skybox = mat;

		#region Fog

		var fogState = container.GetBooleanProperty(RoomPropertyDefs.Room_Fog).Current.Translating(fs => fs.Value);
		var fogDensity = container.GetScalarProperty(RoomPropertyDefs.Room_Fog_Density).Current.Translating(fd => fd.Value);

		var combinedFogDensity = new MultiTranslatingSetting<bool, double, double>(fogState, fogDensity, (fogEnabled, density) => !fogEnabled ? 0 : density);
		var combinedFogDensityBnd = combinedFogDensity.Bind(SetFogDensity);
		combinedFogDensityBnd.DisposeOnDestroy(this);

		var fogColorBnd = container.GetColorProperty(RoomPropertyDefs.Room_Fog_Color_Primary)
			.Bind(SetFogPrimaryColor);
		fogColorBnd.DisposeOnDestroy(this);

		

		#endregion

		#region Skybox

		var skyboxBnd = container.GetLinkProperty(RoomPropertyDefs.Room_Skybox)
			.Bind(UpdateSkyboxSource);
		skyboxBnd.DisposeOnDestroy(this);

		var skyboxIntensityBnd = container.GetScalarProperty(RoomPropertyDefs.Room_SkyboxIntensity)
			.Bind(SetSkyboxIntensity);
		skyboxIntensityBnd.DisposeOnDestroy(this);

		var skyboxRotBnd = container.GetScalarProperty(RoomPropertyDefs.Room_SkyboxRotation)
			.Bind(SetSkyboxRotation);
		skyboxRotBnd.DisposeOnDestroy(this);

		var skyboxTintBnd = container.GetColorProperty(RoomPropertyDefs.Room_SkyboxTint).Bind(SetSkyboxTint);
		skyboxTintBnd.DisposeOnDestroy(this);
		#endregion
	}

	private LinkTargetAssetManager<TextureWithUVs> linkStateManager = null;

	public async void UpdateSkyboxSource(PropertyId propId)
	{
		linkStateManager?.Dispose();

		linkStateManager = new LinkTargetAssetManager<TextureWithUVs>(propId,
			roomPropsContainer.Parent, false, SetSkyboxToTex, BindToProgressStatus);
	}

	private void BindToProgressStatus(IProgressStatusView prog) { }


	private void SetSkyboxToTex(TextureWithUVs tex)
	{
		if (tex == null)
		{
			//mat.SetTexture("_Tex", null);
		}
		else
		{
			mat.SetTexture("_Tex", tex.Texture);
			mat.SetFloat("_XScale", tex.UVRect.width);
			mat.SetFloat("_YScale", tex.UVRect.height);
		}
	}

	private void SetSkyboxIntensity(double intensity)
	{
		RenderSettings.ambientIntensity = (float)intensity;
		mat.SetFloat("_Exposure", (float)intensity);
	}

	private void SetSkyboxRotation(double obj)
	{
		mat.SetFloat("_Rotation", (float)obj);
	}

	private void SetSkyboxTint(Color4F color)
	{
		mat.SetColor("_Tint", color.ToColor());
	}
	
	private void SetFogDensity(double density)
	{
		var d = (float)density;
		RenderSettings.fogDensity = d / 10;
		mat?.SetFloat(CavrnusSkyboxProperties.FogDensity, d);
	}

	private void SetFogPrimaryColor(Color4F color)
	{
		RenderSettings.fogColor = color.ToColor();
		mat?.SetColor(CavrnusSkyboxProperties.FogColor, color.ToColor());
	}

	public event Action Disposed;

	public void Dispose()
	{
		Disposed?.Invoke();
		linkStateManager?.Dispose();
	}
}
