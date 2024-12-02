using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy;
using Collab.Proxy.Content;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ContainerContext;
using Collab.Proxy.Prop.JournalInterop;
using UnityBase.LiveUnityManagement;
using UnityBase.PropRenderers;
using UnityBase.UnityServices;
using UnityEngine;
using UnityEngine.Video;
using Mesh = UnityEngine.Mesh;

namespace UnityBase.Content
{
	public class HoloToLiveUnityObject : IDisposable
	{
		private HoloRoot holoSrc;
		private GameObject rootObject;
		private HoloComponentAccessor hca;
		private PropertySetManager rootContainer;
		private IThumbnailRenderer thumbnailService;
		private Func<PropertyId, string, IEvaluatedPolicyHandle> genPolicyRes;

		private List<IDisposable> disposables = null;

		public HoloToLiveUnityObject(HoloRoot holoSrc, GameObject rootObject, PropertySetManager rootContainer, IThumbnailRenderer thumbnailService, Func<PropertyId, string, IEvaluatedPolicyHandle> genPolicyRes)
		{
			this.holoSrc = holoSrc;
			this.rootObject = rootObject;
			this.hca = rootObject.GetComponent<HoloComponentAccessor>();
			this.rootContainer = rootContainer;
			this.thumbnailService = thumbnailService;
			this.genPolicyRes = genPolicyRes;
		}

		public void Dispose()
		{
			disposables?.ForEach(d => d?.Dispose());
			disposables = null;
		}

		private T Reg<T>(T d) where T : IDisposable
		{
			(disposables = disposables ?? new List<IDisposable>()).Add(d);
			return d;
		}

		public void LoadToLiveOb()
		{
			Set<HoloComponentIdentifier> enumeratedSet = new Set<HoloComponentIdentifier>();
			foreach (var node in holoSrc.EnumerateAllChildrenDepthAndDependencyFirst(enumeratedSet))
				LoadComponentToLive(node.Component);
		}
		
		private void LoadComponentToLive(IHoloStreamComponent component)
		{
			var realComponent = hca.IdComponents.GetFirstByKey(component.ComponentId.ComponentId.ToString());
			IPropertyBoundManager pbm = null;
			if (component is LightHoloComponent lch)
				pbm = LoadLightComponent(lch, realComponent as Light);
			if (component is GenericMaterialAssetHoloStreamComponent mathsc)
				pbm = LoadMaterialComponent(mathsc, realComponent as Material);
			if (component is TextureAssetHoloStreamComponent atahsc && realComponent is Texture rcTex)
				pbm = LoadTextureComponent(atahsc, rcTex);
			if (component is TextureAssetHoloStreamComponent atahscv && realComponent is VideoPlayer vp)
				pbm = LoadVideoTextureComponent(atahscv, vp);
			if (component is CubeTextureAssetHoloStreamComponent cubec && realComponent is Cubemap cube)
				pbm = LoadCubeTextureComponent(cubec, cube);
			if (component is GeometryAssetHoloStreamComponent geoc && realComponent is Mesh mesh)
				pbm = LoadGeometryComponent(geoc, mesh);
			if (component is ProceduralGeometryAssetHoloStreamComponent pgeoc && realComponent is Mesh mesh2)
				pbm = LoadProceduralGeometryComponent(pgeoc, mesh2);

			if (pbm != null)
			{
				Reg(pbm);
				hca.propManagers.Add(new Pair<string, IPropertyBoundManager>(component.ComponentId.ComponentId.ToString(), pbm));
			}
		}

		private IPropertyBoundManager LoadLightComponent(LightHoloComponent c, Light ob)
		{
			var integration = new UnityLightImplementation(ob);
			var initToProps = new LightInitDataToProps(c, ob.gameObject.name);
			var propsManager = new LightPropertiesBindings(integration, initToProps, c.ParentComponentId.ComponentId.ToString());
			return propsManager;
		}

		private IPropertyBoundManager LoadMaterialComponent(GenericMaterialAssetHoloStreamComponent c, Material ob)
		{
			var matContainer = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityMaterialImplementation(ob);
			integration.SetAssetFetchLogic(matContainer);
			var initToProps = new MaterialInitDataToProps(c, ob.name);

			var propsManager = new MaterialPropertyBindings(integration, initToProps, genPolicyRes);

			Reg(matContainer.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(matContainer.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_Material, PropertyDefs.PropertyContext_PartType_Meta));

			Reg(matContainer.AddContext(new HoloMaterialContext(ob)));
			Reg(matContainer.AddContext(new MaterialThumbnailContext(integration, thumbnailService)));

			return propsManager;
		}

		private IPropertyBoundManager LoadGeometryComponent(GeometryAssetHoloStreamComponent c, Mesh mesh)
		{
			var container = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityGeometryImplementation(mesh);
			var propsManager = new GeometryPropertyBindings(integration);

			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_Geometry, PropertyDefs.PropertyContext_PartType_Meta));

			return propsManager;
		}

		private IPropertyBoundManager LoadProceduralGeometryComponent(ProceduralGeometryAssetHoloStreamComponent c, Mesh mesh)
		{
			var container = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityGeometryImplementation(mesh);
			var propsManager = new GeometryPropertyBindings(integration);

			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_Geometry, PropertyDefs.PropertyContext_PartType_Meta));

			return propsManager;
		}

		private IPropertyBoundManager LoadTextureComponent(TextureAssetHoloStreamComponent c, Texture ob)
		{
			var container = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityHoloTextureImplementation(ob, c);
			var propsManager = new TexturePropertyBindings(integration, new TextureInitDataToProps(c));

			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_Texture, PropertyDefs.PropertyContext_PartType_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartTexture_Type,
				c.TextureCategory.ToString().ToLowerInvariant(), PropertyDefs.PropertyContext_PartTexture_Type_Meta));

			Reg(container.AddContext(new HoloPartPropertyContainerContext(container.AbsoluteId, PropertyDefs.PropertyContext_PartType_Texture)));

			// TODO BH: Dispose of the definitions when the object is released

			return propsManager;
		}

		private IPropertyBoundManager LoadCubeTextureComponent(CubeTextureAssetHoloStreamComponent c, Cubemap ob)
		{
			var container = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityHoloCubeTextureImplementation(ob);
			var propsManager = new CubeTexturePropertyBindings(integration, new CubeTextureInitDataToProps(c));

			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_CubeTexture, PropertyDefs.PropertyContext_PartType_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartTexture_Type,
				PropertyDefs.PropertyContext_PartTexture_Type_Colormap, PropertyDefs.PropertyContext_PartTexture_Type_Meta));

			Reg(container.AddContext(new HoloPartPropertyContainerContext(container.AbsoluteId, PropertyDefs.PropertyContext_PartType_CubeTexture)));

			// TODO BH: Dispose of the definitions when the object is released

			return propsManager;
		}

		private IPropertyBoundManager LoadVideoTextureComponent(TextureAssetHoloStreamComponent c, VideoPlayer ob)
		{
			var container = rootContainer.SearchForContainer(new PropertyId(c.ComponentId.ComponentId.ToString()));

			var integration = new UnityVideoTextureImplementation(ob, c);
			var propsManager = new VideoTexturePropertyBindings(integration, new VideoTextureInitDataToProps(c));

			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_Type,
				PropertyDefs.PropertyContext_Type_Part, PropertyDefs.PropertyContext_Type_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartType,
				PropertyDefs.PropertyContext_PartType_VideoTexture, PropertyDefs.PropertyContext_PartType_Meta));
			Reg(container.DefineStringProperty(PropertyDefs.PropertyContext_PartTexture_Type,
				c.TextureCategory.ToString().ToLowerInvariant(), PropertyDefs.PropertyContext_PartTexture_Type_Meta));

			Reg(container.AddContext(new HoloPartPropertyContainerContext(container.AbsoluteId, PropertyDefs.PropertyContext_PartType_VideoTexture)));
			// TODO BH: Dispose of the definitions when the object is released

			return propsManager;
		}
	}
}
