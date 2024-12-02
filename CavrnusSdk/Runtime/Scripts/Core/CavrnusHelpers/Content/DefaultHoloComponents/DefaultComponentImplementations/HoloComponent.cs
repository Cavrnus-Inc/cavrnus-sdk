using System;
using System.Collections;
using System.Collections.Generic;
using Collab.Base.Graphics;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content.DefaultHoloComponents
{
	public interface IInteractibleComponent { }

	public interface IUnityToHolo
	{
		HoloComponentIdentifier ConvertMaterial(Material mat, HoloRoot root, Texture optionalLightmap, Vector4? optionalLightmapUvs);
		HoloComponentIdentifier ConvertTexture(Texture tex, HoloRoot root, Image2D.ImageTextureTypeEnum type);

		HoloComponentIdentifier SearchForComponent(UnityEngine.Object unityOb);
		HoloComponentIdentifier SearchForOrConvertComponent(UnityEngine.Object unityOb, GameObject context, HoloRoot t, HoloNode parent);

		void AfterHierarchy(Action after);
		void AfterAnimations(Action after);
	}

	public abstract class HoloComponent : MonoBehaviour
	{
		[SerializeField] private List<string> componentTags = null;
		public List<string> Tags { get { return componentTags; } set { componentTags = value; } }

		public abstract HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo conversions);
	}
}