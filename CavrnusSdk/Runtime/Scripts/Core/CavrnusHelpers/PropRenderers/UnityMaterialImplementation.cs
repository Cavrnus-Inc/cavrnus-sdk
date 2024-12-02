using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ContainerContext;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.LinkProp;
using UnityEngine;

namespace UnityBase.PropRenderers
{
	public class UnityMaterialImplementation : IMaterialPropertyRenderer
	{
		public event Action<Material> materialChanged;
		public Material material;

		private Func<Texture, Image2D.ImageTextureTypeEnum, HoloComponentIdentifier> writeTextureToHolo;

		private bool enable_TransparencyMode = true;
		private string memory_TransparencyMode = "opaque";

		private string shader = "";
		private IShaderDescription shaderDesc = null;

		public UnityMaterialImplementation(Material mat, Func<Texture, Image2D.ImageTextureTypeEnum, HoloComponentIdentifier> writeTextureToHolo = null)
		{
			this.material = mat;
			this.shader = this.material?.shader?.name;
			if (!String.IsNullOrWhiteSpace(this.shader))
				this.shaderDesc = ShaderDescriptions.Instance.LookupShader(this.shader);
			this.writeTextureToHolo = writeTextureToHolo;
		}

		private PropertySetManager matPropsManager;
		public void SetAssetFetchLogic(PropertySetManager rootPropertiesManager)
		{
			this.matPropsManager = rootPropertiesManager;
		}
		private HoloComponentAccessor componentAccessor;
		public void SetAssetFetchLogic(HoloComponentAccessor componentAccessor)
		{
			this.componentAccessor = componentAccessor;
		}

		public void SetName(string name)
		{
			this.material.name = name;
		}

		public string GetName()
		{
			return this.material.name;
		}

		public void SetShader(string shader)
		{
			//If we can't find it, this handles the default
			shaderDesc = ShaderDescriptions.Instance.LookupShader(shader);
			shader = shaderDesc?.Key ?? shader;

			material.shader = Shader.Find(shader);

			//DebugOutput.Info($"Setting shader ({shader}) for mat {material}");

			materialChanged?.Invoke(material);
		}

		public string GetShader()
		{
			return this.shader;
		}

		public void SetScalarValue(string key, double val)
		{
			if (material == null || material.shader == null)
				return;

			if (material.HasProperty(key))
				material.SetFloat(key, (float)val);
			materialChanged?.Invoke(material);
		}

		public double GetScalarValue(string key)
		{
			if (!material.HasProperty(key) && CommonMaterialKeywords.StrengthMaterialCorrelations.ContainsKey(key))
			{
				var texKey = CommonMaterialKeywords.StrengthMaterialCorrelations[key];
				if (material.HasProperty(texKey))
				{
					return material.GetTexture(texKey) != null ? 1 : 0;
				}
			}

			return material.GetFloat(key);
		}

		public void SetColorValue(string key, Color4F val)
		{
			//DebugOutput.Info($"Setting color {key} on mat {material} to value {val}");
			if (material == null || material.shader == null)
				return;

			if (material.HasProperty(key))
				material.SetColor(key, val.ToColor());
			materialChanged?.Invoke(material);
		}

		public Color4F GetColorValue(string key)
		{
			return material.GetColor(key).ToColor4F();
		}

		public void SetVectorValue(string key, Float4 val)
		{
			if (material == null || material.shader == null)
				return;

			if (material.HasProperty(key))
				material.SetVector(key, val.ToVec4());
			materialChanged?.Invoke(material);
		}

		public Float4 GetVectorValue(string key)
		{
			return material.GetVector(key).ToFloat4();
		}

		private Dictionary<string, IDisposable> textureUVBindings = new Dictionary<string, IDisposable>();
		private Dictionary<string, LinkTargetAssetManager<TextureWithUVs>> textureBindingDisposables = new Dictionary<string, LinkTargetAssetManager<TextureWithUVs>>();
		public void SetTextureValue(string key, PropertyId val)
		{
			//DebugOutput.Info($"Setting texture {key} on mat {material} to value {val}");
			if (material == null || material.shader == null)
				return;

			if (componentAccessor != null)
			{
				//We do this because the HoloComponentAccessor isnt fully set up yet, so we can't use it's helpers.
				//TODO: Fix that!
				var tex = componentAccessor.IdComponents.Components.FirstOrDefault(c => val != null && c.Id == val.MainId())?.Object as Texture;
				if (tex != null && material.HasProperty(key))
					material.SetTexture(key, tex);
			}
			else
			{
				if (textureBindingDisposables.ContainsKey(key))
					textureBindingDisposables[key]?.Dispose();
				if (textureUVBindings.ContainsKey(key))
					textureUVBindings[key]?.Dispose();
				if (textureUVs.ContainsKey(key))
					textureUVs.Remove(key);

				if (!val.IsEmpty)
				{
					var texUvsProp = matPropsManager.SearchForContainer(val).GetVectorProperty(TexturePropertyDefs.Textures_TextureUvs);
					textureUVBindings[key] = texUvsProp.Bind((v) =>
					{
						textureUVs[key] = v;
						SetActualTextureRect(key);
					});
				}

				textureBindingDisposables[key] = new LinkTargetAssetManager<TextureWithUVs>(val, matPropsManager, false, tex =>
				{
					if (material == null || material.shader == null)
						return;

					if (material.HasProperty(key))
						material.SetTexture(key, tex?.Texture);
				}, prog => { });
			}
			materialChanged?.Invoke(material);
		}

		public HoloComponentIdentifier GetTextureValue(string key)
		{
			if (writeTextureToHolo == null)
				return null;

			var texEntry = this.shaderDesc?.TexturePropAndShaderKeys?.FirstOrDefault(tp => tp.ShaderKeyword == key);

			Image2D.ImageTextureTypeEnum type = Image2D.ImageTextureTypeEnum.Colormap;
			if (texEntry != null)
				type = (texEntry.Metadata?.Edit?.ContentType as TextureContentType)?.Usage.ToImageContentType() ?? 
						MaterialComponentExtensions.GuessTextureTypeByPropertyName(key);

			var tex = material.GetTexture(key);
			if (type == Image2D.ImageTextureTypeEnum.Smoothnessmap)
			{
				if (material.HasInt("_WorkflowMode") && material.GetInt("_WorkflowMode") == 0) // specular mode uses gamma smoothness maps, for some reason
				{
					type = Image2D.ImageTextureTypeEnum.Colormap; // stay gamma!
				}
			}
			return writeTextureToHolo(tex, type);
		}


		public void SetTextureRect(string key, Float4 val)
		{
			materialUVs[key] = val;

			SetActualTextureRect(key);
		}

		private Dictionary<string, Float4> textureUVs = new Dictionary<string, Float4>();
		private Dictionary<string, Float4> materialUVs = new Dictionary<string, Float4>();
		private void SetActualTextureRect(string key)
		{
			if (material == null || material.shader == null) 
				return;

			var texRect = Float4.BasicRect;
			if (textureUVs.ContainsKey(key))
				texRect = textureUVs[key];

			var matRect = Float4.BasicRect;
			if (materialUVs.ContainsKey(key))
				matRect = materialUVs[key];

			if (material.HasProperty(key))
			{
				material.SetTextureOffset(key, new Vector2(matRect.x * texRect.z + texRect.x, matRect.y * texRect.w + texRect.y));
				material.SetTextureScale(key, new Vector2(matRect.z * texRect.z, matRect.w * texRect.w));
			}

			materialChanged?.Invoke(material);
		}

		public Float4 GetTextureRectValue(string key)
		{
			var offset = material.GetTextureOffset(key);
			var scale = material.GetTextureScale(key);

			return new Float4(offset.x, offset.y, scale.x, scale.y);
		}

		public void UpdateRenderOrder()
		{
			var rq = ShaderDescriptions.Instance.LookupShader(material.shader.name)?.CalculateRenderOrder(material.shaderKeywords) ?? 2000;
			material.renderQueue = rq;
		}

		public void SetKeyword(string keyword, bool val)
		{
			if (val)
				material.EnableKeyword(keyword);
			else
				material.DisableKeyword(keyword);
			
			UpdateRenderOrder();

			materialChanged?.Invoke(material);
		}

		public bool GetKeywordValue(string key)
		{
			return material.IsKeywordEnabled(key);
		}

		public void SetCustomValue(string keyword, string val)
		{
			if (keyword == MaterialPropertyDefs.Materials_OpacityMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_OpacityMode_Opaque:
					default:
						material.DisableKeyword(CommonMaterialKeywords._ALPHATEST_ON);
						material.DisableKeyword(CommonMaterialKeywords._ALPHABLEND_ON);
						material.DisableKeyword(CommonMaterialKeywords._ALPHAPREMULTIPLY_ON);
						material.DisableKeyword(CommonMaterialKeywords._SURFACE_TYPE_TRANSPARENT);
						material.SetFloat(CommonMaterialKeywords._ZWrite, 1f);
						SetBlendVal(CommonMaterialKeywords._SrcBlend,
							MaterialPropertyDefs.Materials_BlendCoeff_One);
						SetBlendVal(CommonMaterialKeywords._DstBlend,
							MaterialPropertyDefs.Materials_BlendCoeff_Zero);
						enable_TransparencyMode = false;
						material.renderQueue = 2000;
						material.SetFloat(CommonMaterialKeywords._Surface, 0f);
						material.SetFloat(CommonMaterialKeywords._Blend, 3f);
						break;
					case MaterialPropertyDefs.Materials_OpacityMode_Transparent:
						material.DisableKeyword(CommonMaterialKeywords._ALPHATEST_ON);
						material.EnableKeyword(CommonMaterialKeywords._ALPHABLEND_ON);
						material.DisableKeyword(CommonMaterialKeywords._ALPHAPREMULTIPLY_ON);
						material.EnableKeyword(CommonMaterialKeywords._SURFACE_TYPE_TRANSPARENT);
						material.SetFloat(CommonMaterialKeywords._ZWrite, 0f);
						material.renderQueue = 3000;
						// Re-enable and apply transparency mode
						enable_TransparencyMode = true;
						SetCustomValue(MaterialPropertyDefs.Materials_TransparencyBlend_Mode, memory_TransparencyMode);
						material.SetFloat(CommonMaterialKeywords._Surface, 1f);
						material.SetFloat(CommonMaterialKeywords._Blend, 0f);
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_WorkflowMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_WorkflowMode_Metallic:
						material.DisableKeyword(CommonMaterialKeywords._SPECULAR_SETUP);
						break;
					case MaterialPropertyDefs.Materials_WorkflowMode_Specular:
						material.EnableKeyword(CommonMaterialKeywords._SPECULAR_SETUP);
						break;
					default:
						break;
				}
			}
			else if (keyword == MaterialPropertyDefs.Materials_TransparencyBlend_Mode)
			{
				memory_TransparencyMode = val;
				if (!enable_TransparencyMode)
				{
					return;
				}
				switch (val)
				{
					case MaterialPropertyDefs.Materials_TransparencyBlend_None:
					default:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_One);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_Zero);
						material.SetFloat(CommonMaterialKeywords._Blend, 0f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Alpha:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha);
						material.SetFloat(CommonMaterialKeywords._Blend, 0f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Additive:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_One);
						material.SetFloat(CommonMaterialKeywords._Blend, 2f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Multiply:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_DstColor);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_Zero);
						material.SetFloat(CommonMaterialKeywords._Blend, 3f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Premultiply:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_One);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha);
						material.SetFloat(CommonMaterialKeywords._Blend, 1f);
						material.EnableKeyword(CommonMaterialKeywords._ALPHAPREMULTIPLY_ON);
						material.DisableKeyword(CommonMaterialKeywords._ALPHABLEND_ON);
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_ColorBlendMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_ColorBlend_Additive:
						material.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						material.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						material.EnableKeyword(CommonMaterialKeywords._COLORADDSUBDIFF_ON);
						material.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Multiply:
						material.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						material.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						material.DisableKeyword(CommonMaterialKeywords._COLORADDSUBDIFF_ON);
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Subtractive:
						material.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						material.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						material.EnableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						material.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(-1.0f, 0.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Overlay:
						material.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						material.DisableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						material.EnableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Difference:
						material.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						material.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						material.EnableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						material.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(-1.0f, 1.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Color:
						material.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						material.DisableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						material.EnableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_SrcBlend)
			{
				SetBlendVal(CommonMaterialKeywords._SrcBlend, val);
			}
			else if (keyword == MaterialPropertyDefs.Materials_DstBlend)
			{
				SetBlendVal(CommonMaterialKeywords._DstBlend, val);
			}
		}

		private void SetBlendVal(string keyword, string val)
		{
			switch (val)
			{
				case MaterialPropertyDefs.Materials_BlendCoeff_One:
				default:
					material.SetFloat(keyword, 1);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_Zero:
					material.SetFloat(keyword, 0);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha:
					material.SetFloat(keyword, 5);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha:
					material.SetFloat(keyword, 10);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_DstAlpha:
					material.SetFloat(keyword, 7);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstAlpha:
					material.SetFloat(keyword, 8);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcAlphaSaturate:
					material.SetFloat(keyword, 9);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcColor:
					material.SetFloat(keyword, 3);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_DstColor:
					material.SetFloat(keyword, 2);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcColor:
					material.SetFloat(keyword, 6);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstColor:
					material.SetFloat(keyword, 4);
					break;
			}
		}

		public string GetCustomValue(string keyword)
		{
			if (keyword == MaterialPropertyDefs.Materials_OpacityMode)
			{
				if (material.IsKeywordEnabled(CommonMaterialKeywords._SURFACE_TYPE_TRANSPARENT) ||
					material.GetInt(CommonMaterialKeywords._Surface) == 1)
					return MaterialPropertyDefs.Materials_OpacityMode_Transparent;
				return MaterialPropertyDefs.Materials_OpacityMode_Opaque;
			}

			if (keyword == MaterialPropertyDefs.Materials_TransparencyBlend_Mode)
			{
				var srcBlend = material.GetFloat(CommonMaterialKeywords._SrcBlend);
				var dstBlend = material.GetFloat(CommonMaterialKeywords._DstBlend);


				if (srcBlend == 5 && dstBlend == 10)
					return MaterialPropertyDefs.Materials_TransparencyBlend_Alpha;
				else if (srcBlend == 5 && dstBlend == 1)
					return MaterialPropertyDefs.Materials_TransparencyBlend_Additive;
				else if (srcBlend == 2 && dstBlend == 0)
					return MaterialPropertyDefs.Materials_TransparencyBlend_Multiply;
				else if (srcBlend == 1 && dstBlend == 10)
					return MaterialPropertyDefs.Materials_TransparencyBlend_Premultiply;
				else
					return MaterialPropertyDefs.Materials_TransparencyBlend_Alpha;

			}

			if (keyword == MaterialPropertyDefs.Materials_ColorBlendMode)
			{
				if (material.IsKeywordEnabled(CommonMaterialKeywords._COLORADDSUBDIFF_ON)) //switch based on vec4
				{
					var blend = material.GetVector(CommonMaterialKeywords._BaseColorAddSubDiff);
					switch (blend)
					{
						case { x: 1 }:
							return MaterialPropertyDefs.Materials_ColorBlend_Additive;
						case { x: -1, y: 0 }:
							return MaterialPropertyDefs.Materials_ColorBlend_Subtractive;
						case { x: -1, y: 1 }:
							return MaterialPropertyDefs.Materials_ColorBlend_Difference;
					}
				}

				if (material.IsKeywordEnabled(CommonMaterialKeywords._COLORCOLOR_ON))
					return MaterialPropertyDefs.Materials_ColorBlend_Color;
				if (material.IsKeywordEnabled(CommonMaterialKeywords._COLOROVERLAY_ON))
					return MaterialPropertyDefs.Materials_ColorBlend_Overlay;
				if (material.IsKeywordEnabled(CommonMaterialKeywords._COLORCOLOR_ON))
					return MaterialPropertyDefs.Materials_ColorBlend_Multiply;

				if (!material.IsKeywordEnabled(CommonMaterialKeywords._COLOROVERLAY_ON) &&
						!material.IsKeywordEnabled(CommonMaterialKeywords._COLORCOLOR_ON) &&
						!material.IsKeywordEnabled(CommonMaterialKeywords._COLORADDSUBDIFF_ON))
				{
					return MaterialPropertyDefs.Materials_ColorBlend_Multiply;
				}

			}

			if (keyword == MaterialPropertyDefs.Materials_SrcBlend)
			{
				return ReadBlendVal(CommonMaterialKeywords._SrcBlend);
			}

			if (keyword == MaterialPropertyDefs.Materials_WorkflowMode)
			{
				if (material.IsKeywordEnabled(CommonMaterialKeywords._SPECULAR_SETUP))
					return MaterialPropertyDefs.Materials_WorkflowMode_Specular;
				return MaterialPropertyDefs.Materials_WorkflowMode_Metallic;
			}

			return keyword == MaterialPropertyDefs.Materials_DstBlend ? ReadBlendVal(CommonMaterialKeywords._DstBlend) : null;
		}

		private string ReadBlendVal(string keyword)
		{
			switch (material.GetFloat(keyword))
			{
				case 1:
				default:
					return MaterialPropertyDefs.Materials_BlendCoeff_One;
				case 0:
					return MaterialPropertyDefs.Materials_BlendCoeff_Zero;
				case 5:
					return MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha;
				case 10:
					return MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha;
				case 7:
					return MaterialPropertyDefs.Materials_BlendCoeff_DstAlpha;
				case 8:
					return MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstAlpha;
				case 9:
					return MaterialPropertyDefs.Materials_BlendCoeff_SrcAlphaSaturate;
				case 3:
					return MaterialPropertyDefs.Materials_BlendCoeff_SrcColor;
				case 2:
					return MaterialPropertyDefs.Materials_BlendCoeff_DstColor;
				case 6:
					return MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcColor;
				case 4:
					return MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstColor;
			}
		}
	}
}

