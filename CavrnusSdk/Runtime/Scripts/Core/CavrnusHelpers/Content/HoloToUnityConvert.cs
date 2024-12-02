using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Math;
using Collab.Base.Scheduler;
using Collab.Holo;
using Collab.Holo.HoloComponents;
using Collab.Proxy.Prop.JournalInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityBase.Content.DefaultHoloComponents.DefaultComponentImplementations;
using UnityBase.DataFlags;
using UnityBase.PropRenderers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;
using Mesh = UnityEngine.Mesh;

namespace UnityBase.Content
{
	public class HoloToUnityConvert
	{
		public HoloToUnityConvert(IHoloComponentFactory factory, IUnityScheduler sched)
		{
			this.factory = factory;
			this.sched = sched;
		}

		public GameObject CreatedRoot { get; private set; }
		public HoloRoot LoadingHolo { get; private set; }

		public HoloComponentAccessor ComponentAccessor;
		public Dictionary<HoloComponentIdentifier, UnityEngine.Object> componentMap = new Dictionary<HoloComponentIdentifier, UnityEngine.Object>();

		private IProcessFeedback pf;

		private IHoloComponentFactory factory;
		private readonly IUnityScheduler sched;

		private Material defaultMaterial = null;

		public void InitializeLoading(HoloRoot _holo, GameObject rootParent)
		{
			this.LoadingHolo = _holo;

			CreatedRoot = new GameObject(LoadingHolo.RootMetadata?.ObjectName ?? "Default");
			ComponentAccessor = CreatedRoot.AddComponent<HoloComponentAccessor>();
			ComponentAccessor.TmpHoloRoot = _holo;
			if (_holo.RootMetadata != null)
			{
				if (!_holo.RootMetadata.LeftHanded)
				{
					HandednessConversion hc = new HandednessConversion();
					Dictionary<HoloComponentIdentifier, Pair<Matrix34, Matrix34>> fixupTransforms = new Dictionary<HoloComponentIdentifier, Pair<Matrix34, Matrix34>>();
					hc.ConvertFromRightToLeftHandedAndFixNegativeScales(_holo, fixupTransforms);
					HoloPreprocessing.FixupAllAnimations(_holo, fixupTransforms);
					//				CreatedRoot.transform.localScale = new Vector3(-1, 1, 1);
				}
			}
			if (rootParent != null)
			{
				CreatedRoot.transform.SetParent(rootParent.transform, false);
				CreatedRoot.layer = rootParent.layer;
			}
			componentMap.Add(HoloComponentIdentifier.Nothing, CreatedRoot);
		}

		public void CancelAndDestroy()
		{
			if (CreatedRoot != null && !CreatedRoot.Equals(null))
			{
				var rootRef = CreatedRoot;
				sched.ExecInMainThread(() =>
				{
					GameObject.DestroyImmediate(rootRef);
				});

			}
			CreatedRoot = null;

			foreach (var keyValuePair in componentMap)
			{
				if (!(keyValuePair.Value == null || keyValuePair.Equals(null))) // already destroyed?
				{
					var componentRef = keyValuePair.Value;
					sched.ExecInMainThread(() =>
					{
						UnityEngine.Object.DestroyImmediate(componentRef);
					});
				}
			}

			componentMap.Clear();
		}

		public IEnumerable MakeAllColliders()
		{
			// Triggers imply the necessity of mesh colliders. Boo.
			var meshFilters = CreatedRoot.GetComponentsInChildren<MeshFilter>(true).Concat(CreatedRoot.GetComponents<MeshFilter>()); // find all children with mesh filters.
			foreach (var meshFilter in meshFilters)
			{
				try
				{
					if (HelperFunctions.NullOrDestroyed(meshFilter) || HelperFunctions.NullOrDestroyed(meshFilter.sharedMesh))
						continue;

					factory.ConstructColliderFor(meshFilter);
				}
				catch(Exception e)
				{
					Debug.LogError($"Exception during meshFilter collider construction: {e}");
					continue;
				}
				yield return new Helpers.TimingInfo(meshFilter.sharedMesh.vertexCount > 16384 ? Helpers.TimingInfo.Difficulty.Hard : Helpers.TimingInfo.Difficulty.OK);
			}

			var skinnedMeshFilters = CreatedRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			if (skinnedMeshFilters.Length > 0)
			{
				foreach (var smr in skinnedMeshFilters)
				{
					try
					{
						if (HelperFunctions.NullOrDestroyed(smr) || HelperFunctions.NullOrDestroyed(smr.sharedMesh))
							continue;
						factory.ConstructColliderFor(smr);
					}
					catch (Exception e)
					{
						Debug.LogError($"Exception during skinnedMeshFilter collider construction: {e}");
						continue;
					}
					yield return new Helpers.TimingInfo(smr.sharedMesh.vertexCount > 16384 ? Helpers.TimingInfo.Difficulty.Hard : Helpers.TimingInfo.Difficulty.OK);
				}
			}
			yield return null;
		}

		public async Task LoadComponent(IHoloStreamComponent component, IProcessFeedback pf)
		{
			this.pf = pf;

			UnityEngine.Object parent = null;
			if (!componentMap.TryGetValue(component.ParentComponentId, out parent))
				parent = CreatedRoot;

			await LoadComponentNow(component, parent as GameObject);

			if (componentMap.ContainsKey(component.ComponentId))
			{
				ComponentAccessor.IdComponents.Components.Add(new HoloTagObjectLookup(component.ComponentId.ComponentId.ToString(), componentMap[component.ComponentId]));
				if (component.Tags != null)
					foreach (string tag in component.Tags)
						ComponentAccessor.TaggedComponents.Components.Add(new HoloTagObjectLookup(tag, componentMap[component.ComponentId]));

				if (component is INamedHoloStreamComponent)
					ComponentAccessor.NamedComponents.Components.Add(new HoloTagObjectLookup((component as INamedHoloStreamComponent).Name, componentMap[component.ComponentId]));
			}
		}

		public async Task LoadComponentNow(IHoloStreamComponent component, GameObject parentAsGO = null)
		{
			if (componentMap.ContainsKey(component.ComponentId)) // already processed
				return;

			foreach (var dependencyId in component.Dependencies ?? new List<HoloComponentIdentifier>())
			{
				if (!componentMap.ContainsKey(dependencyId))
				{
					await LoadComponentNow(LoadingHolo.NodesById[dependencyId].Component);
				}
			}

			if (HelperFunctions.NullOrDestroyed(parentAsGO))
				return;
			if (HelperFunctions.NullOrDestroyed(CreatedRoot))
				return;

			UnityEngine.Object c = null;

			try
			{
				if (component is BlindDataStreamComponent)
					c = LoadBlindNode(component as BlindDataStreamComponent, parentAsGO);
				else if (component is TransformNodeHoloStreamComponent)
					c = LoadTransformNode(component as TransformNodeHoloStreamComponent, parentAsGO);
				else if (component is ObjectHoloStreamComponent)
					c = LoadObjectNode(component as ObjectHoloStreamComponent, parentAsGO);
				else if (component is GeometryAssetHoloStreamComponent)
					c = LoadGeometryNode(component as GeometryAssetHoloStreamComponent);
				else if (component is ProceduralGeometryAssetHoloStreamComponent)
					c = LoadProceduralGeometryNode(component as ProceduralGeometryAssetHoloStreamComponent);
				else if (component is GenericMaterialAssetHoloStreamComponent)
					c = LoadMaterialNode(component as GenericMaterialAssetHoloStreamComponent);
				else if (component is TextureAssetHoloStreamComponent)
					c = await LoadTextureNode(component as TextureAssetHoloStreamComponent, parentAsGO);
				else if (component is CubeTextureAssetHoloStreamComponent)
					c = await LoadCubemapNode(component as CubeTextureAssetHoloStreamComponent);
				else if (component is MetadataHoloStreamComponent) // INCLUDES root metadata
					c = LoadMetadataNode(component as MetadataHoloStreamComponent, parentAsGO);
				else if (component is AnimationClipHoloComponent)
					c = LoadAnimationClipComponent(component as AnimationClipHoloComponent, parentAsGO);
				else if (component is ABehaviourHoloComponent)
					c = LoadBehaviourComponent(component as ABehaviourHoloComponent, parentAsGO);
				else if (component is LightHoloComponent)
					c = LoadRealLightComponent(component as LightHoloComponent, parentAsGO);
				else if (component is ReflectionProbeHoloStreamComponent)
					c = LoadReflectionProbeComponent(component as ReflectionProbeHoloStreamComponent, parentAsGO);
				else if (component is AudioAssetHoloStreamComponent)
					c = await LoadAudioAssetComponent(component as AudioAssetHoloStreamComponent, parentAsGO);
				else if (component is TextObjectHoloStreamComponent)
					c = LoadTextObjectComponent(component as TextObjectHoloStreamComponent, parentAsGO);
				else if (component is ImpostorHoloStreamComponent)
					c = LoadImpostorObjectComponent(component as ImpostorHoloStreamComponent, parentAsGO);
				else if (component is CameraHoloStreamComponent)
					c = LoadCameraStreamComponent(component as CameraHoloStreamComponent, parentAsGO);
				else if (component is ParticleHoloComponent)
					c = LoadParticleSystemComponent(component as ParticleHoloComponent, parentAsGO);
				else
					pf.Messages.Warning("Un-handled component type '" + component.GetType().Name + "'.");
			}
			catch (Exception e)
			{
				DebugOutput.Info($"Failure while loading holo component of type '{component.GetType().Name}' [{component.ComponentId}]:\n {e}");
				c = null;
			}

			//If we cancelled before the await came back
			if (HelperFunctions.NullOrDestroyed(CreatedRoot))
				return;

			if (c != null)
				componentMap[component.ComponentId] = c;
		}

		private UnityEngine.Object LoadBehaviourComponent(ABehaviourHoloComponent b, GameObject parentAsGo)
		{
			if (b is GrabbableBehaviourHoloComponent)
			{
				var g = b as GrabbableBehaviourHoloComponent;
				return factory.AddGrabbable(parentAsGo, g.Data);
			}
			else if (b is EyeBehaviourHoloComponent)
			{
				var g = b as EyeBehaviourHoloComponent;
				return factory.AddEye(parentAsGo, g.Data);
			}
			else if (b is SolidSurfaceHoloComponent)
			{
				var g = b as SolidSurfaceHoloComponent;
				return factory.AddSolidSurface(parentAsGo, g.Data);
			}
			else if (b is VisibilityOverrideDisableARHoloComponent)
			{
				var g = b as VisibilityOverrideDisableARHoloComponent;
				return factory.AddVisibilityOverrideDisableAR(parentAsGo, g.Data);
			}
			else if (b is CuttingPlaneHoloStreamComponent)
			{
				var g = b as CuttingPlaneHoloStreamComponent;
				return factory.AddCuttingPlane(parentAsGo, g.Size, g.TagsToSkip, g.BorderColor, g.BorderSize);
			}
			else if (b is ScriptHoloComponent)
			{
				var g = b as ScriptHoloComponent;
				return factory.AddHoloScriptBehavior(parentAsGo, g.Data);
			}
			else if (b is ARTrackerBehaviourHoloComponent)
			{
				var g = b as ARTrackerBehaviourHoloComponent;
				return factory.AddARTrackerComponent(parentAsGo, g.TrackerName, g.TextureId);
			}
			else if (b is FocusLocusHoloComponent)
			{
				var g = b as FocusLocusHoloComponent;
				return factory.AddFocusLocus(parentAsGo, g.Data);
			}
			else if (b is UserStreamSurfaceHoloComponent streamSurface)
			{
				return factory.AddStreamSurface(parentAsGo, streamSurface.Data);
			}
			else if (b is PaintSourceBehaviourHoloComponent)
			{

			}

			return null;
		}

		private UnityEngine.Object LoadMaterialNode(GenericMaterialAssetHoloStreamComponent m)
		{
			var mat = new Material(Shader.Find("Cavrnus/Unlit"));
			var integration = new UnityMaterialImplementation(mat);
			integration.SetAssetFetchLogic(ComponentAccessor);
			var initToOb = new MaterialInitDataToObject(m, integration);
			initToOb.InitDataToOb();
			return mat;
		}

		private UnityEngine.Object LoadRealLightComponent(LightHoloComponent r, GameObject parent)
		{
			var light = parent.AddComponent<Light>();
			var integration = new UnityLightImplementation(light);
			var initToOb = new LightInitDataToObject(r, integration);
			initToOb.InitDataToOb();
			return light;
		}

		private UnityEngine.Object LoadReflectionProbeComponent(ReflectionProbeHoloStreamComponent r, GameObject parent)
		{
			var component = parent.AddComponent<ReflectionProbe>();
			component.mode = ReflectionProbeMode.Realtime;
			component.importance = r.ProbeImportance;
			component.intensity = r.ProbeIntensity;
			component.size = r.ProbeSize.ToVec3();
			component.center = r.ProbeOffset.ToVec3();
			component.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			component.enabled = false;
			component.resolution = r.ProbeResolution;
			component.hdr = r.ProbeHdrMode;
			component.blendDistance = r.ProbeBlendDistance;
			component.boxProjection = r.ProbeBoxReflection;
#if BUILD_MAGICLEAP
			component.hdr = false;
#endif
			
			return factory.AddReflectionProbe(parent, component, r.ComponentId);
		}

		private UnityEngine.Object LoadParticleSystemComponent(ParticleHoloComponent p, GameObject parent)
        {
			//Instantiate a new particle system onto the holoRoot GameObject
			var component = parent.AddComponent<ParticleSystem>();

			//Some properties cannot be changed while the system is playing.
			component.Stop();


			//Base properties
			var main = component.main;
			main.duration = p.systemDuration;
			main.loop = p.looping;
			main.prewarm = p.prewarm;
			main.startDelay = LoadMinMaxCurve(p.startDelay);
			main.startLifetime = LoadMinMaxCurve(p.startLifetime);
			main.startSpeed = LoadMinMaxCurve(p.startSpeed);
			main.startSize3D = p.startSize3D;
			if (p.startSize3D)
            {
				main.startSizeX = LoadMinMaxCurve(p.startSizeX);
				main.startSizeY = LoadMinMaxCurve(p.startSizeY);
				main.startSizeZ = LoadMinMaxCurve(p.startSizeZ);
            }
            else
            {
				main.startSize = LoadMinMaxCurve(p.startSizeX);
            }
			main.startRotation = LoadMinMaxCurve(p.startRotation);
			main.flipRotation = p.flipRotation;
			main.startColor = LoadMinMaxGradient(p.startColor);
			main.gravityModifier = LoadMinMaxCurve(p.gravityModifier);
			main.simulationSpace = (ParticleSystemSimulationSpace)p.simulationSpace;

            if (main.simulationSpace == ParticleSystemSimulationSpace.Custom && p.customSimulationSpace.IsValid())
            {
				GameObject customSim = LoadDependencyAllowNull<GameObject>(p.customSimulationSpace);
				if (customSim != null)
					main.customSimulationSpace = customSim.transform;
            }

            main.simulationSpeed = p.simulationSpeed;
			main.useUnscaledTime = p.unscaledTime;
			main.scalingMode = (ParticleSystemScalingMode)p.scalingMode;
			main.playOnAwake = p.playOnAwake;
			main.emitterVelocityMode = (ParticleSystemEmitterVelocityMode)p.emitterVelocityMode;
			if (main.emitterVelocityMode == ParticleSystemEmitterVelocityMode.Custom)
            {
				main.emitterVelocity = new Vector3(p.customVelocity.X,p.customVelocity.Y,p.customVelocity.Z);
            }
			main.maxParticles = p.maxParticles;
			main.stopAction = (ParticleSystemStopAction)p.stopAction;
			main.cullingMode = (ParticleSystemCullingMode)p.cullingMode;
			main.ringBufferMode = (ParticleSystemRingBufferMode)p.ringBufferMode;
			if (main.ringBufferMode == ParticleSystemRingBufferMode.LoopUntilReplaced)
            {
				main.ringBufferLoopRange = new Vector2(p.ringBufferLoopRange.X, p.ringBufferLoopRange.Y);
            }

			//Emission Properties
			if (p.emissionEnabled)
			{
				var emitter = component.emission;
				emitter.enabled = true;

				emitter.rateOverTime = LoadMinMaxCurve(p.emissionRateOverTime);
				emitter.rateOverDistance = LoadMinMaxCurve(p.emissionRateOverDistance);
				if (p.emissionBurstCount > 0)
                {
					var burstList = new ParticleSystem.Burst[p.emissionBursts.Count];
					for (int i = 0; i < p.emissionBursts.Count; i++)
					{
						var burst = p.emissionBursts[i];
						burstList[i] = new ParticleSystem.Burst(burst.time, (short)burst.count, (short)burst.count, (int)burst.cycles, burst.interval);
						burstList[i].probability = burst.probability;
					}
					emitter.SetBursts(burstList);
                }
			}
			//Shape Properties
			if (p.shapeEnabled)
			{
				var shape = component.shape;
				shape.enabled = true;

				shape.shapeType = (ParticleSystemShapeType)p.shape;
				switch (shape.shapeType)
				{
					case ParticleSystemShapeType.Sphere:
						shape.radius = p.radius;
						shape.radiusThickness = p.radiusThickness;
						shape.arc = p.arc;
						shape.arcMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.arcSpread = p.arcSpread;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Hemisphere:
						shape.radius = p.radius;
						shape.radiusThickness = p.radiusThickness;
						shape.arc = p.arc;
						shape.arcMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.arcSpread = p.arcSpread;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Cone:
					case ParticleSystemShapeType.ConeVolume:	
						shape.angle = p.angle;
						shape.radius = p.radius;
						shape.radiusThickness = p.radiusThickness;
						shape.arc = p.arc;
						shape.arcMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.arcSpread = p.arcSpread;
						shape.length = p.shapeLength;
						//No property for emitFrom enum?! Unity? Hello? Anyone there?
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Donut:
						shape.radius = p.radius;
						shape.donutRadius = p.donutRadius;
						shape.radiusThickness = p.radiusThickness;
						shape.arc = p.arc;
						shape.arcMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.arcSpread = p.arcSpread;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Box:
						//No property for emitFrom enum?! Unity? Hello? Anyone there?
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Mesh:
						shape.meshShapeType = (ParticleSystemMeshShapeType) p.shapeMeshType;
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Vertex)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
						}
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Edge)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
							shape.meshSpawnSpread = p.shapeMeshEdgeSpread;
							shape.meshSpawnSpeed = p.shapeMeshEdgeSpeed;
						}
						shape.meshRenderer = LoadDependencyAllowNull<GameObject>(p.shapeMeshRenderer)?.GetComponent<MeshRenderer>();
						shape.mesh = LoadDependencyAllowNull<Mesh>(p.shapeMesh);
						shape.useMeshMaterialIndex = p.shapeSingleMaterial;
						if (shape.useMeshMaterialIndex)
						{
							shape.meshMaterialIndex = p.shapeMeshMaterialIndex;
						}
						shape.useMeshColors = p.useMeshColors;
						shape.normalOffset = p.shapeNormalOffset;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.MeshRenderer:
						shape.meshShapeType = (ParticleSystemMeshShapeType) p.shapeMeshType;
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Vertex)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
						}
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Edge)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
							shape.meshSpawnSpread = p.shapeMeshEdgeSpread;
							shape.meshSpawnSpeed = p.shapeMeshEdgeSpeed;
						}

                        shape.meshRenderer = LoadDependencyAllowNull<GameObject>(p.shapeMeshRenderer)?.GetComponent<MeshRenderer>();

						shape.useMeshMaterialIndex = p.shapeSingleMaterial;
						if (shape.useMeshMaterialIndex)
						{
							shape.meshMaterialIndex = p.shapeMeshMaterialIndex;
						}
						shape.useMeshColors = p.useMeshColors;
						shape.normalOffset = p.shapeNormalOffset;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.SkinnedMeshRenderer:
						shape.meshShapeType = (ParticleSystemMeshShapeType) p.shapeMeshType;
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Vertex)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
						}
						if (shape.meshShapeType == ParticleSystemMeshShapeType.Edge)
						{
							shape.meshSpawnMode = (ParticleSystemShapeMultiModeValue) p.shapeVertexMode;
							shape.meshSpawnSpread = p.shapeMeshEdgeSpread;
							shape.meshSpawnSpeed = p.shapeMeshEdgeSpeed;
						}
						shape.skinnedMeshRenderer = LoadDependencyAllowNull<GameObject>(p.shapeMeshRenderer)?.GetComponent<SkinnedMeshRenderer>();
						shape.useMeshMaterialIndex = p.shapeSingleMaterial;
						if (shape.useMeshMaterialIndex)
						{
							shape.meshMaterialIndex = p.shapeMeshMaterialIndex;
						}
						shape.useMeshColors = p.useMeshColors;
						shape.normalOffset = p.shapeNormalOffset;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Sprite:
						shape.meshShapeType = (ParticleSystemMeshShapeType) p.shapeMeshType;
						//TODO: o.shapeSprite = shape.Sprite;
						shape.normalOffset = p.shapeNormalOffset;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.SpriteRenderer:
						shape.meshShapeType = (ParticleSystemMeshShapeType) p.shapeMeshType;
						//TODO: o.shapeSpriteRenderer = shape.SpriteRenderer;
						shape.normalOffset = p.shapeNormalOffset;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Circle:
						shape.radius = p.radius;
						shape.radiusThickness = p.radiusThickness;
						shape.arc = p.arc;
						shape.arcMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.arcSpread = p.arcSpread;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.SingleSidedEdge:
						shape.radius = p.radius;
						shape.radiusMode = (ParticleSystemShapeMultiModeValue) p.arcMode;
						shape.radiusSpread = p.arcSpread;
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					case ParticleSystemShapeType.Rectangle:
						if (p.shapeTexture.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
						{
							shape.texture = LoadTextureDependency(p.shapeTexture);
						}
						break;
					default:
						throw new NotImplementedException("Shape Enum missing for Particle Holo Component (HoloToUnityConvert.cs:597)");
				}

				shape.position = new Vector3(p.shapePosition.X, p.shapePosition.Y, p.shapePosition.Z);
				shape.rotation = new Vector3(p.shapeRotation.X, p.shapeRotation.Y, p.shapeRotation.Z);
				shape.scale = new Vector3(p.shapeScale.X, p.shapeScale.Y, p.shapeScale.Z);
				shape.alignToDirection = p.alignToDirection;
				shape.randomDirectionAmount = p.randomizeDirection;
				shape.sphericalDirectionAmount = p.spherizeDirection;
				shape.randomPositionAmount = p.randomizePosition;
			}
			//Velocity
			//Velocity Over Lifetime
			if (p.velocityOverLifetimeEnabled)
            {
				var vol = component.velocityOverLifetime;
				vol.enabled = true;

				vol.x = LoadMinMaxCurve(p.velocityOverLifetimeLinearX);
				vol.y = LoadMinMaxCurve(p.velocityOverLifetimeLinearY);
				vol.z = LoadMinMaxCurve(p.velocityOverLifetimeLinearZ);
				vol.space = (ParticleSystemSimulationSpace)p.velocityOverLifetimeSpace;
				vol.orbitalX = LoadMinMaxCurve(p.velocityOverLifetimeOrbitalX);
				vol.orbitalY = LoadMinMaxCurve(p.velocityOverLifetimeOrbitalY);
				vol.orbitalZ = LoadMinMaxCurve(p.velocityOverLifetimeOrbitalZ);
				vol.orbitalOffsetX = LoadMinMaxCurve(p.velocityOverLifetimeOffsetX);
				vol.orbitalOffsetY = LoadMinMaxCurve(p.velocityOverLifetimeOffsetY);
				vol.orbitalOffsetZ = LoadMinMaxCurve(p.velocityOverLifetimeOffsetZ);
				vol.radial = LoadMinMaxCurve(p.velocityOverLifetimeRadial);
				vol.speedModifier = LoadMinMaxCurve(p.velocityOverLifetimeSpeedModifier);
            }

			//Limit Velocity over Lifetime
			if (p.limitVelocityOverLifetimeEnabled)
            {
				var lvol = component.limitVelocityOverLifetime;
				lvol.enabled = true;

				lvol.separateAxes = p.limitVelocityOverLifetimeSeperateAxis;
				lvol.limitX = LoadMinMaxCurve(p.limitVelocityOverLifetimeSpeedX);
				lvol.limitY = LoadMinMaxCurve(p.limitVelocityOverLifetimeSpeedY);
				lvol.limitZ = LoadMinMaxCurve(p.limitVelocityOverLifetimeSpeedZ);
				lvol.dampen = p.limitVelocityOverLifetimeDampen;
				lvol.drag = LoadMinMaxCurve(p.limitVelocityOverLifetimeDrag);
				lvol.multiplyDragByParticleSize = p.limitVelocityOverLifetimeMultiplyBySize;
				lvol.multiplyDragByParticleVelocity = p.limitVelocityOverLifetimeMultiplyByVelocity;
            }

			//Inherit Velocity
			if (p.inheritVelocityEnabled)
            {
				var iv = component.inheritVelocity;
				iv.enabled = true;

				iv.mode = (ParticleSystemInheritVelocityMode)p.inheritVelocityMode;
				iv.curve = LoadMinMaxCurve(p.inheritVelocityCurve);
            }

			if (p.lifetimeByEmitterSpeedEnabled)
            {
				var les = component.lifetimeByEmitterSpeed;
				les.enabled = true;

				les.curve = LoadMinMaxCurve(p.lifetimeByEmitterSpeedMultiplier);
				les.range = new Vector2(p.lifetimeByEmitterSpeedRange.x, p.lifetimeByEmitterSpeedRange.y);
            }

			if (p.forceOverLifetimeEnabled)
			{
				var fol = component.forceOverLifetime;
				fol.enabled = true;

				fol.x = LoadMinMaxCurve(p.forceOverLifetimeX);
				fol.y = LoadMinMaxCurve(p.forceOverLifetimeY);
				fol.z = LoadMinMaxCurve(p.forceOverLifetimeZ);
				fol.space = (ParticleSystemSimulationSpace)p.forceOverLifetimeSpace;
			}
			//Color over Lifetime
			if (p.colorOverLifetimeEnabled)
            {
				var col = component.colorOverLifetime;
				col.enabled = true;

				col.color = LoadMinMaxGradient(p.colorOverLifetimeColor);
            }

            //Color by Speed
            if (p.colorBySpeedEnabled)
            {
				var cbs = component.colorBySpeed;
				cbs.enabled = true;

				cbs.color = LoadMinMaxGradient(p.colorBySpeedColor);
				cbs.range = new Vector2(p.colorBySpeedRange.X, p.colorBySpeedRange.Y);
            }

			//Size over Lifetime
			if (p.sizeOverLifetimeEnabled)
            {
				var sol = component.sizeOverLifetime;
				sol.enabled = true;

				sol.separateAxes = p.sizeOverLifetimeSeperateAxis;
				if (!sol.separateAxes)
                {
					var curve = LoadMinMaxCurve(p.sizeOverLifetimeSize);
					sol.x = curve;
				}
                else
                {
					var x = LoadMinMaxCurve(p.sizeOverLifetimeSizeX);
					var y = LoadMinMaxCurve(p.sizeOverLifetimeSizeY);
					var z = LoadMinMaxCurve(p.sizeOverLifetimeSizeZ);

					sol.x = x;
					sol.y = y;
					sol.z = z;
                }
            }

			//Size
			if (p.sizeBySpeedEnabled)
            {
				var sbs = component.sizeBySpeed;
				sbs.enabled = true;

				sbs.separateAxes = p.sizeBySpeedSeperateAxis;
				if (!sbs.separateAxes)
                {
					var curve = LoadMinMaxCurve(p.sizeBySpeedSize);
					sbs.size = curve;
                }
				else
                {
					var x = LoadMinMaxCurve(p.sizeBySpeedSizeX);
					var y = LoadMinMaxCurve(p.sizeBySpeedSizeY);
					var z = LoadMinMaxCurve(p.sizeBySpeedSizeZ);

					sbs.x = x;
					sbs.y = y;
					sbs.z = z;
                }
				sbs.range = new Vector2(p.sizeBySpeedRange.X, p.sizeBySpeedRange.Y);
            }
			//Rotation
			if (p.rotationOverLifetimeEnabled)
            {
				var rol = component.rotationOverLifetime; 
				rol.enabled = true;

				rol.separateAxes = p.rotationOverLifetimeSeperateAxes;
				rol.x = LoadMinMaxCurve(p.rotationOverLifeAngularVelocityX);
				rol.y = LoadMinMaxCurve(p.rotationOverLifeAngularVelocityY);
				rol.z = LoadMinMaxCurve(p.rotationOverLifeAngularVelocityZ);
			}
			//Rotation by Speed
			if (p.rotationBySpeedEnabled)
            {
				var rbs = component.rotationBySpeed;
				rbs.enabled = true;

				rbs.separateAxes = p.rotationBySpeedSeperateAxis;
				rbs.x = LoadMinMaxCurve(p.rotationBySpeedAngularVelocityX);
				rbs.y = LoadMinMaxCurve(p.rotationBySpeedAngularVelocityY);
				rbs.z = LoadMinMaxCurve(p.rotationBySpeedAngularVelocityZ);
				rbs.range = new Vector2(p.rotationBySpeedRange.x, p.rotationBySpeedRange.y);
            }
			//External Forces
			if (p.externalForcesEnabled)
			{
				var ef = component.externalForces;
				ef.enabled = true;

				ef.multiplierCurve = LoadMinMaxCurve(p.externalForcesMultiplier);
				ef.influenceFilter = (ParticleSystemGameObjectFilter)p.externalForcesInfluenceFilter;
				if (p.externalForcesFields != null && p.externalForcesFields.Count > 0)
                {
					foreach (var f in p.externalForcesFields)
                    {
						var field = new ParticleSystemForceField();
						field.shape = (ParticleSystemForceFieldShape)f.Shape;
						field.startRange = f.StartRange;
						field.endRange = f.EndRange;
						field.directionX = f.Direction.X;
						field.directionY = f.Direction.Y;
						field.directionZ = f.Direction.Z;
						field.gravity = f.GravityStrength;
						field.gravityFocus = f.GravityFocus;
						field.rotationSpeed = f.RotationSpeed;
						field.rotationAttraction = f.RotationAttraction;
						field.rotationRandomness = new Vector2(f.Randomness.X, f.Randomness.Y);
						field.drag = f.DragStrength;
						field.multiplyDragByParticleSize = f.MultiplyBySize;
						field.multiplyDragByParticleVelocity = f.MultiplyByVelocity;
						field.vectorField = LoadDependencyAllowNull<Texture3D>(f.VolumeTexture);
						field.vectorFieldSpeed = f.VectorSpeed;
						field.vectorFieldAttraction = f.VectorAttraction;

						ef.AddInfluence(field);
                    }
                }
				ef.influenceMask = p.externalForcesInfluenceMask;
            }
			//Noise
			if (p.noiseEnabled)
            {
				var noise = component.noise;
				noise.enabled = true;

				noise.separateAxes = p.noiseSeperateAxes;
				noise.strengthX = LoadMinMaxCurve(p.noiseStrengthX);
				noise.strengthY = LoadMinMaxCurve(p.noiseStrengthY);
				noise.strengthZ = LoadMinMaxCurve(p.noiseStrengthZ);
				noise.frequency = p.noiseFrequency;
				noise.scrollSpeed = LoadMinMaxCurve(p.noiseScrollSpeed);
				noise.damping = p.noiseDamping;
				noise.octaveCount = p.noiseOctaves;
				noise.octaveMultiplier = p.noiseOctaveMultiplier;
				noise.octaveScale = p.noiseOctaveScale;
				noise.quality = (ParticleSystemNoiseQuality)p.noiseQuality;
				noise.remapEnabled = p.noiseRemap;
				noise.positionAmount = LoadMinMaxCurve(p.noisePositionAmount);
				noise.rotationAmount = LoadMinMaxCurve(p.noiseRotationAmount);
				noise.sizeAmount = LoadMinMaxCurve(p.noiseSizeAmount);
            }

			if (p.collisionEnabled)
            {
				var col = component.collision;
				col.enabled = true;

				col.type = (ParticleSystemCollisionType)p.collisionType;
				if (col.type == ParticleSystemCollisionType.Planes)
                {
					//TODO: o.collisionPlaneList = 
                }
				col.mode = (ParticleSystemCollisionMode)p.collisionMode;
				col.dampen = LoadMinMaxCurve(p.collisionDampen);
				col.bounce = LoadMinMaxCurve(p.collisionBounce);
				col.lifetimeLoss = LoadMinMaxCurve(p.collisionLifetimeLoss);
				col.minKillSpeed = p.collisionMinKillSpeed;
				col.maxKillSpeed = p.collisionMaxKillSpeed;
				col.radiusScale = p.collisionRadiusScale;
				col.quality = (ParticleSystemCollisionQuality)p.collisionQuality;
				if (col.quality == ParticleSystemCollisionQuality.High) // Unsupported due to collision complexity
					col.quality = ParticleSystemCollisionQuality.Medium;
				if (col.quality == ParticleSystemCollisionQuality.High)
                {
					col.enableDynamicColliders = p.collisionEnableDynamicCollider;
                }
                else
                {
					col.voxelSize = p.colliderVoxelSize;
                }
				col.maxCollisionShapes = p.collisionMaxShapes;
				col.enableDynamicColliders = p.collisionEnableDynamicCollider;
				col.colliderForce = p.colliderForce;
				col.multiplyColliderForceByCollisionAngle = p.colliderMultiplyByAngle;
				col.multiplyColliderForceByParticleSpeed = p.colliderMultiplyBySpeed;
				col.multiplyColliderForceByParticleSize = p.colliderMultiplyBySize;
				col.sendCollisionMessages = p.sendCollisionMessages;
            }

            //Triggers
            if (p.triggerEnabled)
            {
				var trig = component.trigger;
				trig.enabled = true;
				//TODO: LIst of colliders
				/*foreach (var c in p.triggerList)
				{
					trig.AddCollider(c);
				}*/
				trig.inside = (ParticleSystemOverlapAction)p.triggerInside;
				trig.outside = (ParticleSystemOverlapAction)p.triggerOutside;
				trig.enter = (ParticleSystemOverlapAction)p.triggerEnter;
				trig.exit = (ParticleSystemOverlapAction)p.triggerExit;
				trig.colliderQueryMode = (ParticleSystemColliderQueryMode)p.triggerColliderQueryMode;
				trig.radiusScale = p.triggerRadiusScale;
            }

			//Sub Emitters
			if (p.subEmittersEnabled && p.subEmitters != null && p.subEmitters.Count > 0)
            {
				var se = component.subEmitters;
				se.enabled = true;

				for (var i = 0; i < p.subEmitters.Count; i++)
                {
					var s = p.subEmitters[i];
					var sub = LoadDependency<ParticleSystem>(s.SubEmitter);
					se.AddSubEmitter(sub, (ParticleSystemSubEmitterType)s.Type,(ParticleSystemSubEmitterProperties)s.Properties, s.Probability);
                }

            }

			//Texture Sheets
			if (p.textureSheetEnabled)
            {
				var ts = component.textureSheetAnimation;
				ts.enabled = true;

				ts.mode = (ParticleSystemAnimationMode)p.textureSheetMode;
				if (ts.mode == ParticleSystemAnimationMode.Grid)
                {
					ts.numTilesX = (int)p.textureSheetTiles.X;
					ts.numTilesY = (int)p.textureSheetTiles.Y;
					ts.animation = (ParticleSystemAnimationType)p.textureSheetAnimation;

					if (ts.animation== ParticleSystemAnimationType.SingleRow)
                    {
						ts.rowMode = (ParticleSystemAnimationRowMode)p.textureSheetRowMode;

						if (ts.rowMode == ParticleSystemAnimationRowMode.Custom)
                        {
							ts.rowIndex = p.textureSheetRowIndex;
                        }
					}
				}
				
				ts.timeMode = (ParticleSystemAnimationTimeMode)p.textureSheetTimeMode;

				if (ts.timeMode == ParticleSystemAnimationTimeMode.Lifetime)
                {
					ts.frameOverTime = LoadMinMaxCurve(p.textureSheetFrameOverTime);
                }
				if (ts.timeMode == ParticleSystemAnimationTimeMode.Speed)
                {
					ts.speedRange = new Vector2(p.textureSheetSpeedRange.x, p.textureSheetSpeedRange.y);
                }
				if (ts.timeMode == ParticleSystemAnimationTimeMode.FPS)
                {
					ts.fps = p.textureSheetFPS;
                }
				ts.frameOverTime = LoadMinMaxCurve(p.textureSheetFrameOverTime);
				ts.startFrame = LoadMinMaxCurve(p.textureSheetStartFrame);
				ts.cycleCount = p.textureSheetCycles;
				ts.uvChannelMask = (UVChannelFlags) p.textureSheetUVChannels;
            }

			//Lights
			if (p.lightEnabled)
            {
				var light = component.lights;
				light.enabled = true;

				light.light = LoadDependencyAllowNull<Light>(p.light);
				light.ratio = p.lightRatio;
				light.useRandomDistribution = p.lightRandomDistribution;
				light.useParticleColor = p.lightUseParticleColor;
				light.sizeAffectsRange = p.lightSizeAffectsRange;
				light.alphaAffectsIntensity = p.lightAlphaAffectsIntensity;
				light.range = LoadMinMaxCurve(p.lightRangeMulti);
				light.intensity = LoadMinMaxCurve(p.lightIntensityMulti);
				light.maxLights = p.lightMaximumCount;
            }

            //Trails
            if (p.trailEnabled)
            {
				var trail = component.trails;
				trail.enabled = true;

				trail.mode = (ParticleSystemTrailMode) p.trailMode;

				if (trail.mode == ParticleSystemTrailMode.PerParticle)
                {
					trail.ratio = p.trailRatio;
					trail.lifetime = LoadMinMaxCurve(p.trailLifetime);
					trail.minVertexDistance = p.trailMinVertexDistance;
					trail.worldSpace = p.trailWorldSpace;
					trail.dieWithParticles = p.trailDieWithParticles;
                }
				if(trail.mode == ParticleSystemTrailMode.Ribbon)
                {
					trail.ribbonCount = p.trailRibbonCount;
					trail.splitSubEmitterRibbons = p.trailSplitSubEmitterRibbons;
					trail.attachRibbonsToTransform = p.trailAttachRibbonsToTransform;
                }

				trail.textureMode = (ParticleSystemTrailTextureMode) p.trailTextureMode;
				trail.sizeAffectsWidth = p.trailSizeAffectsWidth;
				trail.sizeAffectsLifetime = p.trailSizeAffectsLifetime;
				trail.inheritParticleColor = p.trailInheritColor;
				trail.colorOverLifetime = LoadMinMaxGradient(p.trailColorOverLifetime);
				trail.widthOverTrail = LoadMinMaxCurve(p.trailWidthOverTrail);
				trail.colorOverTrail = LoadMinMaxGradient(p.trailColorOverTrail);
				trail.generateLightingData = p.trailGenerateLightingData;
				trail.shadowBias = p.trailShadowBias;
            }

			//Custom Data 
			if (p.customDataEnabled)
            {
				var cd = component.customData;
				cd.enabled = true;

            }
			//Render Properties
			var ren = component.GetComponent<ParticleSystemRenderer>();
			ren.enabled = p.renderEnabled;

			ren.renderMode = (ParticleSystemRenderMode)p.renderMode;

			//Trail Render
			if (p.trailEnabled)
			{
				ren.trailMaterial = componentMap[p.trailMaterial] as Material;
			}

			//Mesh Render mode specific settings
			if (ren.renderMode == ParticleSystemRenderMode.Mesh)
			{
				ren.mesh = LoadDependencyAllowNull<Mesh>(p.mesh);
				ren.enableGPUInstancing = p.enableMeshGPUInstancing;
			}

			ren.sharedMaterial = componentMap[p.Material] as Material;
			ren.sortMode = (ParticleSystemSortMode)p.sortMode;
			ren.normalDirection = p.normalDirection;
			ren.sortingFudge = p.sortingFudge;
			ren.minParticleSize = p.minParticleSize;
			ren.maxParticleSize = p.maxParticleSize;
			ren.alignment = (ParticleSystemRenderSpace)p.renderAlignment;
			ren.flip = new Vector3(p.flip.X, p.flip.Y, p.flip.Z);
			ren.allowRoll = p.allowRoll;
			ren.pivot = new Vector3(p.pivot.X, p.pivot.Y, p.pivot.Z);
			ren.maskInteraction = (SpriteMaskInteraction)p.masking;
			ren.shadowCastingMode = (ShadowCastingMode)p.castShadows;
			ren.shadowBias = p.shadowBias;
			ren.sortingOrder = p.orderInLayer;
			ren.lightProbeUsage = (LightProbeUsage)p.lightProbes;
			ren.reflectionProbeUsage = (ReflectionProbeUsage)p.reflectionProbes;
			
			return factory.AddParticleSystem(parent, component, p.ComponentId);
        }

		private async Task<UnityEngine.Object> LoadAudioAssetComponent(AudioAssetHoloStreamComponent audioHolo, GameObject parent)
		{
			return await factory.AddAudioData(parent, audioHolo, sched);
		}
		
		private UnityEngine.Object LoadTextObjectComponent(TextObjectHoloStreamComponent t, GameObject parent)
		{
			return factory.AddTextComponent(parent, t.Text, t.FontSize, t.FontColor, t.Alignment, t.HorizontalOverflow, t.VerticalOverflow, t.TextAreaWidth, t.TextAreaHeight, t.ComponentId);
		}

		private UnityEngine.Object LoadCameraStreamComponent(CameraHoloStreamComponent c, GameObject parent)
		{
			return factory.AddCamera(parent, c);
		}

		private UnityEngine.Object LoadImpostorObjectComponent(ImpostorHoloStreamComponent o, GameObject parent)
		{
			var mesh = LoadDependencyAllowNull<Mesh>(o.ImpostorGeometry);
			if (mesh == null)
				throw new ArgumentNullException("LoadImpostor: Geometry is not present.");

			var textures = o.ImpostorImages.Select(LoadDependency<Texture2D>).ToList();
			if (textures.Count != 4 || textures.Any(t => t == null))
				throw new ArgumentNullException("LoadImpostor: Invalid Textures. Must have 4 valid textures.");

			Shader impostorShader = null;
			if (o.Parameters.ImpostorType == ImpostorParameters.ImpostorTypeEnum.Spherical)
				impostorShader = Shader.Find("Custom/LoD/Spherical Impostor");
			else if (o.Parameters.ImpostorType == ImpostorParameters.ImpostorTypeEnum.Octahedral ||
					 o.Parameters.ImpostorType == ImpostorParameters.ImpostorTypeEnum.HemiOctahedral)
				impostorShader = Shader.Find("Custom/LoD/Octahedral Impostor");

			if (impostorShader == null)
				throw new NotSupportedException("Impostor shaders not found. Impostor cannot load.");

			GameObject go = new GameObject(o.Name);
			go.SetActive(o.DefaultActive);

			var mf = go.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh;

			var mat = new Material(impostorShader);
			if (o.Parameters.ImpostorType == ImpostorParameters.ImpostorTypeEnum.Spherical)
			{
				mat.SetFloat("_FramesX", o.Parameters.FramesX);
				mat.SetFloat("_FramesY", o.Parameters.FramesY);
			}
			else
			{
				mat.SetFloat("_Frames", o.Parameters.FramesX);
				mat.SetFloat("_Parallax", o.Parameters.Parallax);
				mat.SetFloat("_Hemi", o.Parameters.Hemi);
			}

			mat.SetFloat("_AI_ShadowBias", o.Parameters.ShadowBias);
			mat.SetFloat("_AI_ShadowView", o.Parameters.ShadowView);
			mat.SetFloat("_ClipMask", o.Parameters.ClipMask);
			mat.SetFloat("_DepthSize", o.Parameters.DepthSize);
			mat.SetFloat("_Hue", o.Parameters.Hue);
			mat.SetFloat("_ImpostorSize", o.Parameters.ImpostorSize);
			mat.SetFloat("_TextureBias", o.Parameters.TextureBias);
			mat.SetColor("_HueVariation", o.Parameters.HueVariation.ToColor());
			mat.SetColor("_Offset", o.Parameters.Offset.ToColor());
			mat.SetTexture("_Albedo", textures[0]);
			mat.SetTextureOffset("_Albedo", Vector2.zero);
			mat.SetTextureScale("_Albedo", Vector2.one);
			mat.SetTexture("_Emission", textures[1]);
			mat.SetTextureOffset("_Emission", Vector2.zero);
			mat.SetTextureScale("_Emission", Vector2.one);
			mat.SetTexture("_Normals", textures[2]);
			mat.SetTextureOffset("_Normals", Vector2.zero);
			mat.SetTextureScale("_Normals", Vector2.one);
			mat.SetTexture("_Specular", textures[3]);
			mat.SetTextureOffset("_Specular", Vector2.zero);
			mat.SetTextureScale("_Specular", Vector2.one);

			var mr = go.AddComponent<MeshRenderer>();
			mr.sharedMaterial = mat;
			mr.allowOcclusionWhenDynamic = true;
			mr.receiveShadows = false;
			mr.shadowCastingMode = ShadowCastingMode.On;

			go.transform.SetParent(parent.transform, false);
			go.SetLayerRecursive(parent.layer);

			SetTransform(go, o.OffsetTransform);

			return go;
		}

		private MetaDataComponent LoadMetadataNode(MetadataHoloStreamComponent m, GameObject parentAsGo)
		{
			if (m is RootMetadataHoloStreamComponent)
			{
				CreatedRoot.name = (m as RootMetadataHoloStreamComponent).ObjectName;
			}

			if (m.Metadata?.Any() ?? false)
			{
				MetaDataComponent mc = parentAsGo.AddComponent<MetaDataComponent>();
				mc.MetaData = m.Metadata.Select(inMetaData => new MetaDataEntry() { Key = inMetaData.Key, Value = inMetaData.Value })
					.ToList();
				return mc;
			}
			return null;
		}

		private T GetNodeYieldAs<T>(HoloComponentIdentifier id)
			where T : UnityEngine.Object
		{
			UnityEngine.Object ob;
			if (componentMap.TryGetValue(id, out ob))
			{
				if (ob is T)
					return (T)ob;
				else
				{
					if (ob != null)
					{
						DebugOutput.Warning($"Tried to access node '{id}' yield as type '{typeof(T)?.Name}', but is '{ob?.GetType()?.Name}'.");
					}
					return default(T);
				}
			}
			DebugOutput.Warning($"Tried to access node '{id}' yield, but there was none. Node was either not yet processed, or had no yield.");

			return default(T);
		}

		private Texture2D LoadTextureDependency(HoloComponentIdentifier componentId)
		{
			if (componentId == null || !componentId.IsValid())
				return null;

			var dep = LoadDependency<UnityEngine.Object>(componentId);
			if (dep is Texture2D tex)
				return tex;
			else if (dep is VideoPlayer vp)
				return vp.texture as Texture2D;
			return null;
		}
		private T LoadDependencyAllowNull<T>(HoloComponentIdentifier componentId)
			where T : UnityEngine.Object
		{
			if (componentId == null || !componentId.IsValid())
				return null;

			else return LoadDependency<T>(componentId);
		}

		private T LoadDependency<T>(HoloComponentIdentifier componentId)
			where T : UnityEngine.Object
		{
			Debug.Assert(componentMap.ContainsKey(componentId));
			return GetNodeYieldAs<T>(componentId);
		}


		private GameObject LoadTransformNode(TransformNodeHoloStreamComponent t, GameObject parent)
		{
			GameObject transformNode = new GameObject(String.IsNullOrEmpty(t.Name) ? t.ComponentId.ComponentId.ToString() : t.Name);
			transformNode.SetActive(t.DefaultActive);
			transformNode.transform.SetParent(parent.transform, false);
			transformNode.layer = parent.layer;

			SetTransform(transformNode, t.Transform);

			if (t is SwitchTransformNodeStreamComponent s)
			{
				factory.AddSwitchTransformHandler(transformNode, s.ChildrenVisibility);
			}

			if (t.Tags != null && t.Tags.Count > 0)
			{
				HoloTag tags = transformNode.AddComponent<HoloTag>();
				tags.tags = t.Tags.ToArray();
			}

			return transformNode;
		}

		public GameObject LoadObjectNode(ObjectHoloStreamComponent o, GameObject parent)
		{
			var geometryMesh = LoadDependencyAllowNull<Mesh>(o.Geometry);
			List<Material> matObs = new List<Material>();

			GameObject go = new GameObject(o.Name);
			go.AddComponent<MeshRenderer>();

			foreach (var matId in o.Materials)
			{
				matObs.Add(componentMap[matId] as Material);
			}

			//factory.PullMaterialFromObjectOnPart(LoadDependencyAllowNull<UnityEngine.Object>(o.Material), go);
			if (matObs.Count == 0)
			{
				DebugOutput.Log("Trying to load object: " + o.Name + " with no assigned Material.  This used to be a default, but not it's just showing as invisible.");
				if (defaultMaterial == null)
				{
					defaultMaterial = new Material(Shader.Find("Cavrnus/Lit"));
					defaultMaterial.color = (Color4F.White * .8f).ToColor();
				}
				matObs.Add(defaultMaterial);
			}

			go.SetActive(o.DefaultActive);

			if (o is SkinnedObjectHoloStreamComponent so)
			{
				var smr = go.AddComponent<SkinnedMeshRenderer>();
				smr.sharedMesh = geometryMesh;
				smr.sharedMaterials = matObs.ToArray();
				smr.receiveShadows = o.MRSettings.RecieveShadows;
				smr.shadowCastingMode = (ShadowCastingMode)o.MRSettings.ShadowMode;
				smr.allowOcclusionWhenDynamic = false;
				smr.lightProbeUsage = LightProbeUsage.Off;
				
				if (so.BoneTargets != null)
				{
					var bones = so.BoneTargets.Select((bt => GetNodeYieldAs<GameObject>(bt).transform)).ToArray();
					smr.bones = bones;
				}

				if (so.RootTransform.ComponentId != HoloComponentIdentifier.Nothing.ComponentId)
				{
					smr.rootBone = parent.transform;
					smr.rootBone = GetNodeYieldAs<GameObject>(so.RootTransform)?.transform;
				}

				smr.quality = SkinQuality.Bone4;
				smr.updateWhenOffscreen = false;
				
			}
			else
			{
				// Plain old object

				var mf = go.AddComponent<MeshFilter>();
				mf.sharedMesh = geometryMesh;

				if (matObs.Count > 0)
				{
					var mr = go.GetComponent<MeshRenderer>();
					mr.sharedMaterials = matObs.ToArray();
					mr.receiveShadows = o.MRSettings.RecieveShadows;
					mr.shadowCastingMode = (ShadowCastingMode) o.MRSettings.ShadowMode;
					mr.allowOcclusionWhenDynamic = false; // no runtime OC support.
					mr.lightProbeUsage = LightProbeUsage.Off; // no light probes ( until LLV maybe? Or Custom provided, from holo plugin)
				}
			}

			go.transform.SetParent(parent.transform, false);
			go.layer = parent.layer;

			if (!o.ReceiveRaycast)
				go.AddComponent<PointingIgnoreFlag>();

			SetTransform(go, o.OffsetTransform);
			
			if (o.Tags != null && o.Tags.Count > 0)
			{
				HoloTag tags = go.AddComponent<HoloTag>();
				tags.tags = o.Tags.ToArray();
			}
			if (o.Name == "CollisionSphere")
			{
				var mesh = go.GetComponent<MeshRenderer>();
				mesh.enabled = false;
			};
			return go;
		}

		private Mesh LoadGeometryNode(GeometryAssetHoloStreamComponent g)
		{
			return MeshConvert.ConstructUnityMesh(g.Mesh);
		}

		private Mesh LoadProceduralGeometryNode(ProceduralGeometryAssetHoloStreamComponent g)
		{
			return MeshConvert.ConstructUnityMesh(g.Mesh.BuildProceduralMesh());
		}

		private async Task<UnityEngine.Object> LoadTextureNode(TextureAssetHoloStreamComponent t, GameObject parent)
		{
			if (t.IsVideo())
			{
				var player = await TextureConvertFromHolo.ConvertVideoTexture(t, parent, this.sched);
				return player;
			}
			else
			{
				var tex = await TextureConvertFromHolo.ConvertTexture(t, this.sched, false);
				//ComponentAccessor.Textures.Add(Tuple.Create(TextureAssetMetadata.FromTextureAsset(t), tex));
				return tex;
			}
		}

		private Task<Cubemap> LoadCubemapNode(CubeTextureAssetHoloStreamComponent c)
		{
			return TextureConvertFromHolo.ConvertToCubemapTexture(c);
		}

		private AnimationClip LoadAnimationClipComponent(AnimationClipHoloComponent a, GameObject parentAsGo)
		{
			var ac = AnimationConvert.ConvertToAnimationClip(a, parentAsGo, GetNodeYieldAs<UnityEngine.Object>);
			factory.AddAnimationClip(parentAsGo, ac, a);
			return ac;
		}

		private BlindDataComponent LoadBlindNode(BlindDataStreamComponent blindDataStreamComponent, GameObject parentOb)
		{
			if (parentOb == null)
			{
				DebugOutput.Error("Loading blind node with parent '" + blindDataStreamComponent.ParentComponentId + "', which is not a gameobject. Blind data wi");
				return null;
			}

			var b = parentOb.AddComponent<BlindDataComponent>();
			b.HoloType = blindDataStreamComponent.ComponentTypeId;
			b.BlindData = blindDataStreamComponent.blindBytes;

			return b;
		}
		private ParticleSystem.MinMaxCurve LoadMinMaxCurve(ParticleMinMaxCurve mmc)
        {
			var c = new ParticleSystem.MinMaxCurve();
			c.mode = (ParticleSystemCurveMode)mmc.Mode;
			if (c.mode == ParticleSystemCurveMode.Curve)
			{
				c.curve = new AnimationCurve();
				
				foreach (var k in mmc.Keys)
                {
					var key = new Keyframe(k.Time, k.Value, k.InTangent, k.OutTangent);
					c.curve.AddKey(key);
                }
            }
			if (c.mode == ParticleSystemCurveMode.TwoCurves)
            {
	            c.curveMin = new AnimationCurve();
	            c.curveMax = new AnimationCurve();
	            
				foreach (var k in mmc.Keys)
				{
					var key = new Keyframe(k.Time, k.Value, k.InTangent, k.OutTangent);
					c.curveMax.AddKey(key);
				}
				foreach (var k in mmc.KeysMin)
				{
					var key = new Keyframe(k.Time, k.Value, k.InTangent, k.OutTangent);
					c.curveMin.AddKey(key);
				}
			}
			if (c.mode == ParticleSystemCurveMode.Constant)
            {
				c.constant = mmc.Constant;
            }
			if (c.mode == ParticleSystemCurveMode.TwoConstants)
            {
				c.constantMax = mmc.Constant;
				c.constantMin = mmc.ConstantMin;
            }
			c.curveMultiplier = mmc.CurveMultiplier;
			return c;
        }
		private ParticleSystem.MinMaxGradient LoadMinMaxGradient(ParticleMinMaxGradient mmg)
        {
			ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
			g.mode = (ParticleSystemGradientMode) mmg.Mode;

			if (g.mode == ParticleSystemGradientMode.Gradient || g.mode == ParticleSystemGradientMode.RandomColor)
			{
				var colorKeys = new GradientColorKey[mmg.GradientColorKeys.Count];
				var colorKeyIndex = 0;
				var alphaKeys = new GradientAlphaKey[mmg.GradientAlphaKeys.Count];
				var alphaKeyIndex = 0;
				foreach (var k in mmg.GradientColorKeys)
				{
					var gradientKey = new GradientColorKey(new Color(k.Value.R, k.Value.G, k.Value.B, k.Value.A), k.Key);
					colorKeys[colorKeyIndex] = gradientKey;
					colorKeyIndex += 1;
				}
				foreach (var k in mmg.GradientAlphaKeys)
				{
					var gradientKey = new GradientAlphaKey(k.Value, k.Key);
					alphaKeys[alphaKeyIndex] = gradientKey;
					alphaKeyIndex += 1;
				}
				g.gradientMax = new Gradient();
				g.gradientMin = new Gradient();
				g.gradient = new Gradient();
				g.gradientMin.SetKeys(colorKeys, alphaKeys);
				g.gradient.SetKeys(colorKeys, alphaKeys);
				g.gradientMax.SetKeys(colorKeys, alphaKeys);
			}

			if (g.mode == ParticleSystemGradientMode.TwoGradients)
			{
				var minColorKeyIndex = 0;
				var minAlphaKeyIndex = 0;
				var ColorKeyIndex = 0;
				var AlphaKeyIndex = 0;

				var ColorKeys = new GradientColorKey[mmg.GradientColorKeys.Count];
				var AlphaKeys = new GradientAlphaKey[mmg.GradientAlphaKeys.Count];
				var minColorKeys = new GradientColorKey[mmg.GradientMinColorKeys.Count];
				var minAlphaKeys = new GradientAlphaKey[mmg.GradientMinAlphaKeys.Count];

				foreach (var k in mmg.GradientMinColorKeys)
				{
					var gradientKey = new GradientColorKey(new Color(k.Value.R, k.Value.G, k.Value.B, k.Value.A), k.Key);
					minColorKeys[minColorKeyIndex] = gradientKey;
					minColorKeyIndex += 1;
				}
				foreach (var k in mmg.GradientMinAlphaKeys)
				{
					var gradientKey = new GradientAlphaKey(k.Value, k.Key);
					minAlphaKeys[minAlphaKeyIndex] = gradientKey;
					minAlphaKeyIndex += 1;
				}
				foreach (var k in mmg.GradientColorKeys)
				{
					var gradientKey = new GradientColorKey(new Color(k.Value.R, k.Value.G, k.Value.B, k.Value.A), k.Key);
					ColorKeys[ColorKeyIndex] = gradientKey;
					ColorKeyIndex += 1;
				}
				foreach (var k in mmg.GradientAlphaKeys)
				{
					var gradientKey = new GradientAlphaKey(k.Value, k.Key);
					AlphaKeys[AlphaKeyIndex] = gradientKey;
					AlphaKeyIndex += 1;
				}
				g.gradientMin = new Gradient();

				//Might need to be gradientMax?
				g.gradient = new Gradient();
				g.gradientMax = new Gradient();
				g.gradientMin.SetKeys(minColorKeys, minAlphaKeys);
				g.gradient.SetKeys(ColorKeys, AlphaKeys);
				g.gradientMax.SetKeys(ColorKeys,AlphaKeys);
			}

			if (g.mode == ParticleSystemGradientMode.Color)
            {
				g.color = mmg.Color.ToColor();
            }

			if (g.mode == ParticleSystemGradientMode.TwoColors)
            {
				g.colorMin = mmg.Color.ToColor();
				g.color = mmg.ColorMax.ToColor();
				g.colorMax = mmg.ColorMax.ToColor();
            }

			return g;
        }

		private void SetTransform(GameObject go, Matrix34 trans)
		{
			TransformMatrixConversion.MatrixToUnityLocal(trans, go.transform);
		}
	}
}
