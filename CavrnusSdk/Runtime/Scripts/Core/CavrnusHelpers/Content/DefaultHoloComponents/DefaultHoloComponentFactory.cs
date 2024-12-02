using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collab.Holo;
using Collab.Base.Math;
using UnityBase;
using UnityBase.Content;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;


namespace UnityBase.Content.DefaultHoloComponents
{
	public class DefaultHoloComponentFactory : IHoloComponentFactory
	{
		public virtual Object AddSwitchTransformHandler(GameObject o, SwitchTransformData data)
		{
			return null;
		}
		
		public virtual async Task<Object> AddAudioData(GameObject o, AudioAssetHoloStreamComponent audioHolo, IUnityScheduler sched)
		{
			// Instantiate on holo root
			var audioSource = await DoLoadAudioComponent(o, audioHolo, sched);
			return audioSource;
		}

		public static async Task<AudioSource> DoLoadAudioComponent(GameObject o, AudioAssetHoloStreamComponent audioHolo, IUnityScheduler sched)
		{
			var audioSource = o.AddComponent<AudioSource>();

			// Read properties
			var length = audioHolo.Length;
			var audioData = audioHolo.AudioData;
			var fileName = audioHolo.FileName;
			var extension = audioHolo.Extension;
			var fileType = audioHolo.FileType;

			var playOnAwake = audioHolo.PlayOnAwake;
			var looping = audioHolo.Looping;

			var pitch = audioHolo.Pitch;
			var panStereo = audioHolo.PanStereo;
			var min = audioHolo.SpatialMin;
			var max = audioHolo.SpatialMax;

			audioSource.playOnAwake = playOnAwake;
			audioSource.loop = looping;

			audioSource.pitch = pitch;
			audioSource.panStereo = panStereo;
			audioSource.minDistance = min;
			audioSource.maxDistance = max;

			audioSource.volume = audioHolo.Volume;
			audioSource.spatialize = audioHolo.Spatial;

			//set to linear falloff until .. some other point in time
			audioSource.rolloffMode = AudioRolloffMode.Linear;

			audioSource.clip = await AudioConvert.ConvertAudioClip(audioHolo, sched);
			return audioSource;
		}

		public virtual Object AddGrabbable(GameObject o, GrabbableData data)
		{
			var component = GetOrAddComponent<GrabbableObject>(o);
			component.MoveType = data.MoveType;
			return component;
		}

		public virtual Object AddSolidSurface(GameObject o, SolidSurfaceData data)
		{
			var component = GetOrAddComponent<SolidSurface>(o);
			component.SurfaceScale = data.SurfaceScale;
			component.TeleportingAllowed = data.TeleportingAllowed;
			return component;
		}

		public virtual Object AddVisibilityOverrideDisableAR(GameObject o, HideInArData data)
		{
			var component = GetOrAddComponent<VisibilityOverrideDisableARComponent>(o);
			return component;
		}

		public virtual Object AddEye(GameObject o, EyeData data)
		{
			var component = GetOrAddComponent<EyeController>(o);
			component.CutoffAngle = data.CutoffAngle;
			component.UseAverageEyePos = data.UseAverageEyePos;
			return component;
		}

		public virtual Object AddHoloScriptBehavior(GameObject o, ScriptData data)
		{
			var component = GetOrAddComponent<HoloScriptBehaviour>(o);
			component.holoScript = data.Script;
			component.scriptVersion = data.ScriptVersion?.ToString() ?? "";
			return component;
		}

		public virtual Object AddCuttingPlane(GameObject o, Float3 cuttingPlaneSize, List<string> tagsToSkip, Color4 borderColor, float borderSize)
		{
			var component = GetOrAddComponent<CuttingPlaneComponent>(o);
			component.CuttingPlaneSize = cuttingPlaneSize.ToVec3();
			component.TagsToSkip = tagsToSkip;
			component.borderSize = borderSize;
			component.borderColor = borderColor.ToColor();
			return component;
		}

		public virtual Object AddCamera(GameObject o, CameraHoloStreamComponent cam)
		{
			var c = GetOrAddComponent<Camera>(o);
			c.enabled = false;
			c.clearFlags = CameraClearFlags.Skybox;
			c.backgroundColor = Color.black;
			c.cullingMask = 0;
			c.fieldOfView = cam.FieldOfViewDegrees;
			c.nearClipPlane = .05f;
			c.farClipPlane = 2000f;

			var component = GetOrAddComponent<CameraStreamComponent>(o);
			component.StreamFrameRate = cam.StreamFrameRate;
			component.StreamResolutionHeight = cam.StreamResolution.y;
			component.StreamResolutionWidth = cam.StreamResolution.x;
			component.UseActiveCameraAspectRatio = cam.UseActiveCameraAspectRatio;
			return null;
		}

		public virtual Object AddFocusLocus(GameObject o, FocusLocusData data)
		{
			var component = GetOrAddComponent<CustomObjectFocusPositionComponent>(o);
			component.RelativeCameraPosition = data.RelativeCameraPos.ToVec3();
			component.ReativeCameraLookPosition = data.RelativeFocusPosition.ToVec3();
			return component;
		}

		public Object AddStreamSurface(GameObject o, UserStreamSurfaceData data)
		{
			var component = GetOrAddComponent<StreamSurfaceComponent>(o);
			component.data = data;
			return component;
		}

		public void AddAnimationClip(GameObject o, AnimationClip ac, AnimationClipHoloComponent achc)
		{
			// Nothing needing doing; in app this needs to retain the transform information to limit which transforms get 'use-animation' locked. 
		}

		public virtual void ConstructColliderFor(MeshFilter meshFilter)
		{
			var foundCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
			if (foundCollider == null)
			{
				var mc = meshFilter.gameObject.AddComponent<MeshCollider>();
				mc.sharedMesh = meshFilter.sharedMesh;
			}
		}

		public virtual void ConstructColliderFor(SkinnedMeshRenderer skinnedMeshRend)
		{
			var foundCollider = skinnedMeshRend.gameObject.GetComponent<MeshCollider>();
			if (foundCollider == null)
			{
				var mc = skinnedMeshRend.gameObject.AddComponent<MeshCollider>();
				mc.sharedMesh = skinnedMeshRend.sharedMesh;
			}
		}

		public virtual Object AddTextComponent(GameObject o, string text, int fontSize, Color4F color, TextObjectHoloStreamComponent.TextAlignmentData alignment, TextObjectHoloStreamComponent.HorizontalOverflowData horizontalOverflow, TextObjectHoloStreamComponent.VerticalOverflowData verticalOverflow, float textAreaWidth, float textAreaHeight, HoloComponentIdentifier compId)
		{
			var component = GetOrAddComponent<HoloTextComponent>(o);
			component.Text = text;
			component.FontSize = fontSize;
			component.FontColor = color.ToColor();
			component.Alignment = alignment;
			component.HorizontalOverflow = horizontalOverflow;
			component.VerticalOverflow = verticalOverflow;
			component.TextAreaWidth = textAreaWidth;
			component.TextAreaHeight = textAreaHeight;
			return component;
		}

		public virtual Object AddARTrackerComponent(GameObject o, string trackerName, HoloComponentIdentifier textureId)
		{
			var component = GetOrAddComponent<HoloArTrackerBehavior>(o);
			component.TrackerName = trackerName;
			return component;
		}

		public virtual Object AddReflectionProbe(GameObject o, ReflectionProbe r, HoloComponentIdentifier compId)
		{
			r.enabled = true;
			return r;
		}

		public virtual Object AddParticleSystem(GameObject o, ParticleSystem ps, HoloComponentIdentifier compId)
        {
			return ps;
        }

		protected T GetOrAddComponent<T>(GameObject o)
			where T : Component
		{
			if (o.GetComponent<T>() == null)
				return o.AddComponent<T>();
			else
				return o.GetComponent<T>();
		}
	}
}




