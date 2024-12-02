using System;
using Collab.Holo;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collab.Base.Math;
using UnityBase;
using UnityBase.Content;
using Float3 = Collab.Base.Math.Float3;
using Object = UnityEngine.Object;

public interface IHoloComponentFactory
{
	Object AddSwitchTransformHandler(GameObject o, SwitchTransformData data);

	Task<Object> AddAudioData(GameObject o, AudioAssetHoloStreamComponent component, IUnityScheduler sched);
	Object AddGrabbable(GameObject o, GrabbableData data);
	Object AddEye(GameObject o, EyeData data);
	Object AddHoloScriptBehavior(GameObject o, ScriptData data);
	Object AddSolidSurface(GameObject o, SolidSurfaceData data);
	Object AddVisibilityOverrideDisableAR(GameObject o, HideInArData data);
	Object AddCuttingPlane(GameObject o, Float3 cuttingPlaneSize, List<string> tagsToSkip, Color4 borderColor, float borderSize);
	Object AddReflectionProbe(GameObject o, ReflectionProbe r, HoloComponentIdentifier compId);
	Object AddParticleSystem(GameObject o, ParticleSystem ps, HoloComponentIdentifier compId);
	Object AddTextComponent(GameObject o, string text, int fontSize, Color4F color, TextObjectHoloStreamComponent.TextAlignmentData alignment
		, TextObjectHoloStreamComponent.HorizontalOverflowData horizontalOverflow, TextObjectHoloStreamComponent.VerticalOverflowData verticalOverflow
		, float textAreaWidth, float textAreaHeight, HoloComponentIdentifier compId);
    Object AddARTrackerComponent(GameObject o, string trackerName, HoloComponentIdentifier textureId);
	
	Object AddCamera(GameObject o, CameraHoloStreamComponent cam);

	Object AddFocusLocus(GameObject o, FocusLocusData data);

	Object AddStreamSurface(GameObject o, UserStreamSurfaceData data);

	void AddAnimationClip(GameObject o, AnimationClip ac, AnimationClipHoloComponent achc);

	void ConstructColliderFor(MeshFilter meshFilter);
	void ConstructColliderFor(SkinnedMeshRenderer skinnedMeshRend);
}