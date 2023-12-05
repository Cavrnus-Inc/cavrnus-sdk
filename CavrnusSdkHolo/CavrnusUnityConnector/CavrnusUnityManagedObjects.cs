using Collab.LiveRoomSystem.GameEngineConnector;
using System;
using System.Collections.Generic;
using Collab.Proxy.Prop;
using Collab.Base.Core;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.TransformProp;
using Collab.Proxy.Prop.LinkProp;
using UnityEngine;
using Collab.Base.Graphics;
using Collab.Base.Math;
using Collab.Base.Scheduler;
using Collab.Proxy.Content;
using System.Threading.Tasks;
using UnityBase;
using UnityBase.Content;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Prop.ContainerContext;

namespace CavrnusSdk
{
	internal class ManagedTransform : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }
		public Transform Trans;

		public ManagedTransform(PropertySetManager container, IManagedEngineObject parent = null)
		{
			Id = container.AbsoluteId;
			Trans = new GameObject("Cavrnus Object").transform;

			if (parent != null)
			{
				if ((parent is ManagedTransform mt))
					Trans.SetParent(mt.Trans, false);
				else
					DebugOutput.Error($"Trying to use {parent} as a parent node, but it is not a ManagedTransform.");
			}

			SetupBindings(container);
		}

		public void SetupBindings(PropertySetManager container)
		{
			var nameBnd = container.GetStringProperty(PropertyDefs.Objects_Name)
				.Bind(s => Trans.gameObject.name = $"{s}");
			nameBnd.DisposeOnDestroy(this);

			var visBnd = container.GetBooleanProperty(PropertyDefs.Objects_Visibility)
				.Bind(v => Trans.gameObject.SetActive(v));
			visBnd.DisposeOnDestroy(this);

			var trnsBnd = container.GetTransformProperty(PropertyDefs.Objects_Transform).Bind(Move);
			trnsBnd.DisposeOnDestroy(this);
		}

		private void Move(TransformComplete trans)
		{
			if (trans != null)
			{
				Trans.localPosition = trans.ResolveTranslation().ToVec3();
				Trans.localEulerAngles = trans.ResolveEuler().ToVec3();
				Trans.localScale = trans.ResolveScaleVector().ToVec3();
			}
		}

		public void Dispose()
		{
			Disposed?.Invoke();
		}

		public event Action Disposed;
	}

	internal class ManagedTransformBindings : APropertyBoundComponent
	{
		private Transform transform;
		public ManagedTransformBindings(Transform transform, PropertySetManager container)
		{
			this.transform = transform;

			var nameBnd = container.GetStringProperty(PropertyDefs.Objects_Name)
				.Bind(s => transform.gameObject.name = $"{s}");
			nameBnd.DisposeOnDestroy(this);

			var visBnd = container.GetBooleanProperty(PropertyDefs.Objects_Visibility)
				.Bind(v => transform.gameObject.SetActive(v));
			visBnd.DisposeOnDestroy(this);

			var trnsBnd = container.GetTransformProperty(PropertyDefs.Objects_Transform).Bind(Move);
			trnsBnd.DisposeOnDestroy(this);
		}

		private void Move(TransformComplete trans)
		{
			if (trans != null)
			{
				transform.localPosition = trans.ResolveTranslation().ToVec3();
				transform.localEulerAngles = trans.ResolveEuler().ToVec3();
				transform.localScale = trans.ResolveScaleVector().ToVec3();
			}
		}
	}

	internal class ManagedGameObject : ManagedTransform
	{
		public GameObject Ob;

		private PropertySetManager container;

		public ManagedGameObject(PropertySetManager container, IManagedEngineObject mesh, List<IManagedEngineObject> materials,
			IManagedEngineObject parent = null) : base(container, parent)
		{
			this.container = container;

			Ob = Trans.gameObject;
			var mf = Ob.AddComponent<MeshFilter>();
			var mr = Ob.AddComponent<MeshRenderer>();

			mf.mesh = (mesh as ManagedMesh)?.Mesh;

			var mats = new List<Material>();
			foreach (var material in materials)
				mats.Add((material as ManagedMaterial)?.Mat);

			mr.sharedMaterials = mats.ToArray();

			SetupObBindings(container);
		}

		public void SetupObBindings(PropertySetManager container)
		{
			SetupBindings(container);

			/*for (int i = 0; i < Ob.GetComponent<MeshRenderer>().sharedMaterials.Length; i++)
			{
				var index = i;
				var materialBnd = container.GetLinkProperty(PropertyDefs.Objects_Material(i))
					.Bind(pId => UpdateMaterialSource(pId, index));
				materialBnd.DisposeOnDestroy(this);
			}*/
		}

		private LinkTargetAssetManager<Material> linkStateManager = null;
		public void UpdateMaterialSource(PropertyId propId, int index)
		{
			linkStateManager?.Dispose();

			linkStateManager = new LinkTargetAssetManager<Material>(propId, container, false, (m) => SetMaterial(m, index), (prog) => { });
		}

		private void SetMaterial(Material mat, int index)
		{
			var sharedMats = Ob.GetComponent<MeshRenderer>().sharedMaterials;
			sharedMats[index] = mat;
			Ob.GetComponent<MeshRenderer>().sharedMaterials = sharedMats;
		}
	}
	
	internal class ManagedMesh : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }
		public UnityEngine.Mesh Mesh;

		public ManagedMesh(PropertySetManager container, Collab.Base.Graphics.Mesh mesh)
		{
			Id = container.AbsoluteId;
			Mesh = MeshConvert.ConstructUnityMesh(mesh);

			var nameBnd = container.GetStringProperty(PropertyDefs.Objects_Name)
				.Bind(s => Mesh.name = $"{s}");
			nameBnd.DisposeOnDestroy(this);
		}

		public void Dispose() => Disposed?.Invoke();
		public event Action Disposed;
	}

	public class TextureAssetContext : AAssetPropertyContext<TextureWithUVs>, ISpecifiedContentType
	{
		private Task<TextureWithUVs> texTask;
		public TextureAssetContext(Task<TextureWithUVs> texTask, TextureContentType.TextureType textureType)
		{
			this.texTask = texTask;
			ContentType = new TextureContentType() { Usage = textureType };
		}

		protected override void Fetch()
		{
			//ProgressOb.Value = new Progress;

			#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			SetTextureFromHandle();
			#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private async Task SetTextureFromHandle()
		{
			if (texTask == null)
				return;

			try
			{
				var res = await texTask;

				if (res != null)
					this.liveAsset.Value = new TextureWithUVs(res.Texture, new Rect(0, 0, 1, 1));
				else
					Debug.Log($"Fetched a null texture from {texTask}");
				ProgressOb.Value = null;
			}
			catch (Exception e)
			{
				DebugOutput.Warning(e.ToString());
				this.liveAsset.Value = null;
				ProgressOb.Value = null;
			}
		}

		protected override void Flush()
		{
			ProgressOb.Value = null;
			this.liveAsset.Value = null;
			texTask = null;
		}

		public ContentType ContentType { get; }
	}

	public class TextureAssetThumbnailContext : AThumbnailPropertyContext<TextureWithUVs>
	{
		private Task<TextureWithUVs> texTask;
		public TextureAssetThumbnailContext(Task<TextureWithUVs> texTask, TextureContentType.TextureType textureType)
		{
			this.texTask = texTask;
			ContentType = new TextureContentType() { Usage = textureType };
		}

		protected override void Fetch()
		{
			//ProgressOb.Value = new Progress;

			#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			SetTextureFromHandle();
			#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private async Task SetTextureFromHandle()
		{
			if (texTask == null)
				return;

			try
			{
				var res = await texTask;
				if (res != null)
					this.liveAsset.Value = new TextureWithUVs(res.Texture, new Rect(0, 0, 1, 1));
				else
					Debug.Log($"Fetched a null texture from {texTask}");
				ProgressOb.Value = null;
			}
			catch (Exception e)
			{
				DebugOutput.Warning(e.ToString());
				this.liveAsset.Value = null;
				ProgressOb.Value = null;
			}
		}

		protected override void Flush()
		{
			ProgressOb.Value = null;
			this.liveAsset.Value = null;
			texTask = null;
		}

		public ContentType ContentType { get; }
	}

	internal class ManagedTexture : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }
		public Task<Texture> Tex;
		private PropertySetManager container;

		public ManagedTexture(PropertySetManager container, CreateTextureRequest texReq, IUnityScheduler scheduler)
		{
			Id = container.AbsoluteId;
			this.container = container;

			Image2D.ImageTextureTypeEnum texType;
			string filteringSet = "trilinear";
			if (texReq.HoloTexReq != null)
			{
				Tex = TextureConvertFromHolo.ConvertTexture(texReq.HoloTexReq, scheduler, false);
				texType = texReq.HoloTexReq.TextureCategory;
				filteringSet = texReq.HoloTexReq.TextureFilter.ToString().ToLowerInvariant();
			}
			else
			{
				texType = texReq.TextureType;
				var isLinear = Image2D.GetIsLinear(texReq.TextureType);
				var canCompress = Image2D.GetCanCompress(texReq.TextureType);
				Tex = TextureConvert.ConvertTextureFromData(texReq.Extension, texReq.Name, texReq.Data, texReq.Length, FilterMode.Bilinear, scheduler, isLinear, canCompress, texReq.CheckRotMetadata);
			}

			container.DefineVectorProperty(TexturePropertyDefs.Textures_TextureUvs, Float4.BasicRect, TexturePropertyDefs.Textures_TextureUvs_Meta);
			container.DefineStringProperty(TexturePropertyDefs.Textures_Filtering, filteringSet, TexturePropertyDefs.Textures_Filtering_Meta);
			container.DefineStringProperty(TexturePropertyDefs.Textures_WrapU, TexturePropertyDefs.Textures_Wrap_Enum.repeat.ToString(), TexturePropertyDefs.Textures_WrapU_Meta);
			container.DefineStringProperty(TexturePropertyDefs.Textures_WrapV, TexturePropertyDefs.Textures_Wrap_Enum.repeat.ToString(), TexturePropertyDefs.Textures_WrapV_Meta);

			container.AddContext(new TextureAssetContext(ToDefaultUVTex(), texType.ToTextureContentType()));
			container.AddContext(new TextureAssetThumbnailContext(ToDefaultUVTex(), texType.ToTextureContentType()));

			LoadTex();
		}

		private async Task<TextureWithUVs> ToDefaultUVTex()
		{
			var tex = await Tex;
			return new TextureWithUVs(tex);
		}

		private Texture resolvedTex;
		private async void LoadTex()
		{
			resolvedTex = await Tex;

			var nameBnd = container.GetStringProperty(PropertyDefs.Objects_Name)
				.Bind(s => resolvedTex.name = $"{s}");
			nameBnd.DisposeOnDestroy(this);

			if (wasDisposed)
			{
				Texture.Destroy(resolvedTex);
				return;
			}
		}

		private bool wasDisposed = false;
		public void Dispose()
		{
			wasDisposed = true;
			if (resolvedTex != null)
				Texture.Destroy(resolvedTex);
			Disposed?.Invoke();
		}

		public event Action Disposed;
	}

	internal class ManagedMaterial : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }
		public Material Mat;

		private PropertySetManager container;

		public ManagedMaterial(PropertySetManager container)
		{
			this.container = container;
			Id = container.AbsoluteId;
			Mat = new Material(Shader.Find("Cavrnus/Lit"));

			SetupBindings(container, Mat);
		}

		public void SetupBindings(PropertySetManager container, Material material)
		{
			var nameBnd = container.GetStringProperty(PropertyDefs.Objects_Name)
				.Bind(s => material.name = $"{s}");
			nameBnd.DisposeOnDestroy(this);

			var shaderBnd = container.GetStringProperty(MaterialPropertyDefs.Materials_Shader).Bind(SetShaderType);
			shaderBnd.DisposeOnDestroy(this);
		}

		private List<IDisposable> currShaderDefs = new List<IDisposable>();

		private void SetShaderType(string shader)
		{
			var newShader = Shader.Find(shader);
			if (newShader != null)
				Mat.shader = newShader;
			else
				DebugOutput.Error($"Failed to find shader in project: {shader}");

			foreach (var shaderDef in currShaderDefs)
				shaderDef.Dispose();
			currShaderDefs.Clear();

			var descr = ShaderDescriptions.Instance.LookupShader(shader);
			if (descr == null)
			{
				DebugOutput.Error($"Failed to find description for shader {shader}");
				return;
			}

			foreach (var p in descr.ColorPropAndShaderKeys)
			{
				var bnd = container.GetColorProperty(p.PropKeyword).Bind(val =>
				{
					if (Mat.HasProperty(p.ShaderKeyword))
						Mat.SetColor(p.ShaderKeyword, val.ToColor());
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.VectorPropAndShaderKeys)
			{
				var bnd = container.GetVectorProperty(p.PropKeyword).Bind(val =>
				{
					if (Mat.HasProperty(p.ShaderKeyword))
						Mat.SetVector(p.ShaderKeyword, val.ToVec4());
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.TexturePropAndShaderKeys)
			{
				var bnd = container.GetLinkProperty(p.PropKeyword).Bind(val =>
				{
					SetTextureValue(p.ShaderKeyword, val);
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.UvPropAndShaderKeys)
			{
				var bnd = container.GetVectorProperty(p.PropKeyword).Bind(val =>
				{
					materialUVs[p.ShaderKeyword] = val;
					SetActualTextureRect(p.ShaderKeyword);
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.ScalarPropAndShaderKeys)
			{
				var bnd = container.GetScalarProperty(p.PropKeyword).Bind(val =>
				{
					if (Mat.HasProperty(p.ShaderKeyword))
						Mat.SetFloat(p.ShaderKeyword, (float)val);
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.KeywordPropAndShaderKeys)
			{
				var bnd = container.GetBooleanProperty(p.PropKeyword).Bind(val =>
				{
					if (val)
						Mat.EnableKeyword(p.ShaderKeyword);
					else
						Mat.DisableKeyword(p.ShaderKeyword);
				});
				currShaderDefs.Add(bnd);
			}
			foreach (var p in descr.CustomPropAndShaderKeys)
			{
				var bnd = container.GetStringProperty(p.PropKeyword).Bind(val =>
				{
					HandleCustomBullshit(p.PropKeyword, val);
				});
				currShaderDefs.Add(bnd);
			}

			foreach (var currShaderDef in currShaderDefs)
				currShaderDef.DisposeOnDestroy(this);
		}

		private Dictionary<string, IDisposable> textureUVBindings = new Dictionary<string, IDisposable>();
		private Dictionary<string, LinkTargetAssetManager<TextureWithUVs>> textureBindingDisposables = new Dictionary<string, LinkTargetAssetManager<TextureWithUVs>>();
		public void SetTextureValue(string key, PropertyId val)
		{
			if (textureBindingDisposables.ContainsKey(key))
				textureBindingDisposables[key]?.Dispose();
			if (textureUVBindings.ContainsKey(key))
				textureUVBindings[key]?.Dispose();
			if (textureUVs.ContainsKey(key))
				textureUVs.Remove(key);

			if (!val.IsEmpty)
			{
				var texUvsProp = container.SearchForContainer(val).GetVectorProperty(TexturePropertyDefs.Textures_TextureUvs);
				textureUVBindings[key] = texUvsProp.Bind((v) =>
				{
					textureUVs[key] = v;
					SetActualTextureRect(key);
				});
			}

			textureBindingDisposables[key] = new LinkTargetAssetManager<TextureWithUVs>(val, container, false, tex =>
			{
				if (Mat.HasProperty(key))
					Mat.SetTexture(key, tex?.Texture);
			}, prog => { });
		}

		private Dictionary<string, Float4> textureUVs = new Dictionary<string, Float4>();
		private Dictionary<string, Float4> materialUVs = new Dictionary<string, Float4>();
		private void SetActualTextureRect(string key)
		{
			var texRect = Float4.BasicRect;
			if (textureUVs.ContainsKey(key))
				texRect = textureUVs[key];

			var matRect = Float4.BasicRect;
			if (materialUVs.ContainsKey(key))
				matRect = materialUVs[key];

			if (Mat.HasProperty(key))
			{
				Mat.SetTextureOffset(key, new Vector2(matRect.x * texRect.z + texRect.x, matRect.y * texRect.w + texRect.y));
				Mat.SetTextureScale(key, new Vector2(matRect.z * texRect.z, matRect.w * texRect.w));
			}
		}

		#region Custom bullshit

		private void HandleCustomBullshit(string keyword, string val)
		{
			if (keyword == MaterialPropertyDefs.Materials_OpacityMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_OpacityMode_Opaque:
					default:
						Mat.DisableKeyword(CommonMaterialKeywords._ALPHATEST_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._ALPHABLEND_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._ALPHAPREMULTIPLY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._SURFACE_TYPE_TRANSPARENT);
						Mat.SetFloat(CommonMaterialKeywords._ZWrite, 1f);
						Mat.SetFloat(CommonMaterialKeywords._CullMode, 0f); //no culling.
						SetBlendVal(CommonMaterialKeywords._SrcBlend,
							MaterialPropertyDefs.Materials_BlendCoeff_One);
						SetBlendVal(CommonMaterialKeywords._DstBlend,
							MaterialPropertyDefs.Materials_BlendCoeff_Zero);
						Mat.renderQueue = 2000;
						Mat.SetFloat(CommonMaterialKeywords._Surface, 0f);
						Mat.SetFloat(CommonMaterialKeywords._Blend, 3f);
						break;
					case MaterialPropertyDefs.Materials_OpacityMode_Transparent:
						Mat.SetFloat(CommonMaterialKeywords._Surface, 1f);
						Mat.DisableKeyword(CommonMaterialKeywords._ALPHATEST_ON);
						Mat.EnableKeyword(CommonMaterialKeywords._ALPHABLEND_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._ALPHAPREMULTIPLY_ON);
						Mat.SetFloat(CommonMaterialKeywords._ZWrite, 0f);
						Mat.renderQueue = 3000;
						//material.SetFloat(CommonMaterialKeywords._CullMode, 2f); //switch to backface cull // NO! Don't! Cull is a distinct property and this overwrites it wi
						Mat.SetFloat(CommonMaterialKeywords._Blend, 0f);
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_WorkflowMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_WorkflowMode_Metallic:
						Mat.DisableKeyword(CommonMaterialKeywords._SPECULAR_SETUP);
						break;
					case MaterialPropertyDefs.Materials_WorkflowMode_Specular:
						Mat.EnableKeyword(CommonMaterialKeywords._SPECULAR_SETUP);
						break;
					default:
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_TransparencyBlend_Mode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_TransparencyBlend_None:
					default:
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Alpha:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha);
						Mat.SetFloat(CommonMaterialKeywords._Blend, 0f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Additive:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_One);
						Mat.SetFloat(CommonMaterialKeywords._Blend, 2f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Multiply:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_DstColor);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_Zero);
						Mat.SetFloat(CommonMaterialKeywords._Blend, 3f);
						break;
					case MaterialPropertyDefs.Materials_TransparencyBlend_Premultiply:
						SetBlendVal(CommonMaterialKeywords._SrcBlend, MaterialPropertyDefs.Materials_BlendCoeff_One);
						SetBlendVal(CommonMaterialKeywords._DstBlend, MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha);
						Mat.SetFloat(CommonMaterialKeywords._Blend, 1f);
						break;
				}
			}

			else if (keyword == MaterialPropertyDefs.Materials_ColorBlendMode)
			{
				switch (val)
				{
					case MaterialPropertyDefs.Materials_ColorBlend_Additive:
						Mat.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						Mat.EnableKeyword(CommonMaterialKeywords._COLORADDSUBDIFF_ON);
						Mat.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Multiply:
						Mat.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._COLORADDSUBDIFF_ON);
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Subtractive:
						Mat.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						Mat.EnableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						Mat.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(-1.0f, 0.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Overlay:
						Mat.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						Mat.EnableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Difference:
						Mat.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
						Mat.EnableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						Mat.SetVector(CommonMaterialKeywords._BaseColorAddSubDiff, new Vector4(-1.0f, 1.0f, 0.0f, 0.0f));
						break;
					case MaterialPropertyDefs.Materials_ColorBlend_Color:
						Mat.DisableKeyword(CommonMaterialKeywords._COLOROVERLAY_ON);
						Mat.DisableKeyword(CommonMaterialKeywords._BaseColorAddSubDiff);
						Mat.EnableKeyword(CommonMaterialKeywords._COLORCOLOR_ON);
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
					Mat.SetFloat(keyword, 1);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_Zero:
					Mat.SetFloat(keyword, 0);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcAlpha:
					Mat.SetFloat(keyword, 5);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcAlpha:
					Mat.SetFloat(keyword, 10);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_DstAlpha:
					Mat.SetFloat(keyword, 7);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstAlpha:
					Mat.SetFloat(keyword, 8);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcAlphaSaturate:
					Mat.SetFloat(keyword, 9);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_SrcColor:
					Mat.SetFloat(keyword, 3);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_DstColor:
					Mat.SetFloat(keyword, 2);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusSrcColor:
					Mat.SetFloat(keyword, 6);
					break;
				case MaterialPropertyDefs.Materials_BlendCoeff_OneMinusDstColor:
					Mat.SetFloat(keyword, 4);
					break;
			}
		}

		#endregion

		public void Dispose() => Disposed?.Invoke();
		public event Action Disposed;
	}


	internal class ManagedLight : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }
		public Light Light;

		public ManagedLight(PropertySetManager container, IManagedEngineObject parent)
		{
			if ((parent is ManagedTransform mt))
				Light = mt.Trans.gameObject.AddComponent<Light>();
			else if ((parent is ManagedGameObject mg))
				Light = mg.Ob.AddComponent<Light>();
			else
				throw new Exception($"Trying to use {parent} as a parent node, but it is not a ManagedTransform.");

			Id = container.AbsoluteId;

			SetupBindings(container, Light);
		}

		private void SetupBindings(PropertySetManager container, Light light)
		{
			var typeBnd = container.GetStringProperty(PropertyDefs.Objects_LightType)
				.BindEnum<LightHoloComponent.LightTypeEnum>(lightType =>
				{
					if (lightType == LightHoloComponent.LightTypeEnum.Directional)
						light.type = LightType.Directional;
					else if (lightType == LightHoloComponent.LightTypeEnum.Spot)
						light.type = LightType.Spot;
					else
						light.type = LightType.Point;
				});
			typeBnd.DisposeOnDestroy(this);

			var shadowBnd = container.GetStringProperty(PropertyDefs.Objects_ShadowMode)
				.BindEnum<LightHoloComponent.ShadowMode>(shadowMode =>
				{
					if (shadowMode == LightHoloComponent.ShadowMode.Hard)
						light.shadows = LightShadows.Hard;
					else if (shadowMode == LightHoloComponent.ShadowMode.Soft)
						light.shadows = LightShadows.Soft;
					else
						light.shadows = LightShadows.None;
				});
			shadowBnd.DisposeOnDestroy(this);

			var intensityBnd = container.GetScalarProperty(PropertyDefs.Objects_LightIntensity)
				.Bind(i => light.intensity = (float)i);
			intensityBnd.DisposeOnDestroy(this);

			var colorBnd = container.GetColorProperty(PropertyDefs.Objects_LightColor)
				.Bind(c => light.color = c.ToColor());
			colorBnd.DisposeOnDestroy(this);

			var indirBnd = container.GetScalarProperty(PropertyDefs.Objects_LightIndirectMultiplier)
				.Bind(ir => light.bounceIntensity = (float)ir);
			indirBnd.DisposeOnDestroy(this);

			var rngBnd = container.GetScalarProperty(PropertyDefs.Objects_LightRange)
				.Bind(lr => light.range = (float)lr);
			rngBnd.DisposeOnDestroy(this);

			var shadowStrBnd = container.GetScalarProperty(PropertyDefs.Objects_ShadowStrength)
				.Bind(shadowStr => light.shadowStrength = (float)shadowStr);
			shadowStrBnd.DisposeOnDestroy(this);

			var lightEnabledBnd = container.GetBooleanProperty(PropertyDefs.Objects_LightEnable)
				.Bind(e => light.enabled = e);
			lightEnabledBnd.DisposeOnDestroy(this);

			//TODO: we should be good to just set these here, and let them be ignored if they aren't relevant?

			var spotAngleOuterBnd = container.GetScalarProperty(PropertyDefs.Objects_SpotAngleOuterDegrees)
				.Bind(outDeg => light.spotAngle = (float)outDeg);
			spotAngleOuterBnd.DisposeOnDestroy(this);

			var spotAngleInnerBnd = container.GetScalarProperty(PropertyDefs.Objects_SpotAngleInnerDegrees)
				.Bind(inDeg => light.innerSpotAngle = (float)inDeg);
			spotAngleInnerBnd.DisposeOnDestroy(this);
		}

		public void Dispose() => Disposed?.Invoke();
		public event Action Disposed;
	}

	internal class ManagedArTracker : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }

		public ManagedArTracker(ManagedTexture tex)
		{

		}

		public void Dispose() => Disposed?.Invoke();
		public event Action Disposed;
	}
}
