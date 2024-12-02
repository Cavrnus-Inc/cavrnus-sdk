using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collab.Base.Collections;
using Collab.Holo;
using Collab.Base.Core;
using Collab.Base.Math;
using UnityEngine;

namespace UnityBase.Content
{
	public interface IHoloLoadedComponentConsideredAMaterial
	{
		GameObject FindUsageOfMaterial();
	}

	public static class AnimationConvert
	{
		public static AnimationClip ConvertToAnimationClip(AnimationClipHoloComponent h, GameObject container, Func<HoloComponentIdentifier, object> getNodeYield)
		{
			var clipLayerRecord = container.GetComponent<AnimationClipLayerRecord>();
			if (clipLayerRecord == null)
			{
				clipLayerRecord = container.AddComponent<AnimationClipLayerRecord>();
			}
			clipLayerRecord.Set(h.Name, h.Layer);

			var animation = container.GetComponent<Animation>();
			if (animation == null)
			{
				animation = container.AddComponent<Animation>();
				animation.playAutomatically = false;
			}

			AnimationClip ac = new AnimationClip();
			ac.name = h.Name;
			ac.legacy = true;
			switch (h.BoundaryBehavior)
			{
				case AnimationSequenceBoundaryBehaviorEnum.Clamp:
					ac.wrapMode = WrapMode.Clamp;
					break;
				case AnimationSequenceBoundaryBehaviorEnum.Loop:
					ac.wrapMode = WrapMode.Loop;
					break;
				case AnimationSequenceBoundaryBehaviorEnum.Stop:
					ac.wrapMode = WrapMode.Once;
					break;
				case AnimationSequenceBoundaryBehaviorEnum.Mirror:
					ac.wrapMode = WrapMode.PingPong;
					break;
			}

            if (h.SingleSequences != null)
            {
                foreach (var sequence in h.SingleSequences)
                {
                    var keyframes = sequence.Keyframes.Select(k => new Keyframe(k.Time, k.Value, k.InTangent, k.OutTangent)).ToArray();
                    AnimationCurve curve = new AnimationCurve(keyframes);
                    curve.postWrapMode = WrapMode.Default;
                    curve.preWrapMode = WrapMode.Default;

                    var modifierComponent = getNodeYield(sequence.TargetParameter.TargetComponent);
                    string parameterName = sequence.TargetParameter.ParameterName;

					// Materials problem here is that the animation is pointing at the material,
					// but unity needs to point at the relevant mesh-renderer's material...

	                if (modifierComponent == null)
						continue;
                    if (modifierComponent is GameObject && parameterName.StartsWith("m_IsActive"))
	                    ac.SetCurve(ResolveAnimationPathTo(container, modifierComponent), typeof(GameObject), parameterName, curve);
                    else if (modifierComponent is Transform || modifierComponent is GameObject)
	                    ac.SetCurve(ResolveAnimationPathTo(container, modifierComponent), typeof(Transform), parameterName, curve);
					else if (modifierComponent is Material mat)
                    {
	                    if (parameterName.Contains("_Color"))
		                    parameterName = parameterName.Replace("_Color", "_BaseColor");

	                    // Find meshrenderers that use this material?
						var renderers = container.GetComponentsInChildren<Renderer>().Where(r => r.sharedMaterial == mat);
						foreach (var renderer in renderers)
						{
							ac.SetCurve(ResolveAnimationPathTo(container, renderer), renderer.GetType(), parameterName, curve);
						}
                    }
					else if (modifierComponent is IHoloLoadedComponentConsideredAMaterial cam)
                    {
	                    var ob = cam.FindUsageOfMaterial();
	                    if (parameterName.Contains("_Color"))
		                    parameterName = parameterName.Replace("_Color", "_BaseColor");

	                    ac.SetCurve(ResolveAnimationPathTo(container, ob), typeof(MeshRenderer), parameterName, curve);
                    }
					else if (modifierComponent is MeshRenderer)
                    {
	                    if (parameterName.Contains("_Color"))
		                    parameterName = parameterName.Replace("_Color", "_BaseColor");

	                    ac.SetCurve(ResolveAnimationPathTo(container, modifierComponent), typeof(MeshRenderer), parameterName, curve);
                    }
					else if (modifierComponent != null) // is ParticleSystem || modifierComponent is AudioSource) // Why not just every other component type?
                    {
	                    ac.SetCurve(ResolveAnimationPathTo(container, modifierComponent), modifierComponent.GetType(), parameterName, curve);
                    }
					/*else
                    {
                        DebugOutput.Warning($"Could not resolve type to animate (" + modifierComponent.GetType() + $"), param '{parameterName}'.");
                    }*/
                }
            }
            if (h.MatrixSequences != null)
            {
                foreach (var sequence in h.MatrixSequences)
				{
					var modifierComponent = getNodeYield(sequence.TargetComponent);
					if (modifierComponent is GameObject)
						modifierComponent = (modifierComponent as GameObject).transform;
					if (!(modifierComponent is Transform))
					{
						// Matrix sequences must be transform
						DebugOutput.Warning("Matrix Animation sequence targetting non-transform node, will be ignored. (Is of type '"+modifierComponent.GetType()+"'.");
						continue;
					}
					string parameterPath = ResolveAnimationPathTo(container, modifierComponent);

					Keyframe[] posx = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] posy = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] posz = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] rotx = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] roty = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] rotz = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] rotw = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] scalex = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] scaley = new Keyframe[sequence.Keyframes.Count];
					Keyframe[] scalez = new Keyframe[sequence.Keyframes.Count];
					Vector3 kpos, kscale;
					Quaternion krot = Quaternion.identity;
					for (int ki = 0;ki<sequence.Keyframes.Count;ki++)
					{
						var k = sequence.Keyframes[ki];
						Quaternion prev = krot;
						TransformMatrixConversion.MatrixToUnity(k.Value, out kpos, out krot, out kscale);
						if (ki > 0)
						{
							if (Quaternion.Dot(prev, krot) < 0f)
								krot = new Quaternion(-krot.x, -krot.y, -krot.z, -krot.w);
						}
						posx[ki] = new Keyframe(k.Time, kpos.x);
						posy[ki] = new Keyframe(k.Time, kpos.y);
						posz[ki] = new Keyframe(k.Time, kpos.z);
						rotx[ki] = new Keyframe(k.Time, krot.x);
						roty[ki] = new Keyframe(k.Time, krot.y);
						rotz[ki] = new Keyframe(k.Time, krot.z);
						rotw[ki] = new Keyframe(k.Time, krot.w);
						scalex[ki] = new Keyframe(k.Time, kscale.x);
						scaley[ki] = new Keyframe(k.Time, kscale.y);
						scalez[ki] = new Keyframe(k.Time, kscale.z);
					}

					bool pxRelevant, pyRelevant, pzRelevant;
					Keyframe[] pxFix = SimplifyAndLinearize(posx, .001f, out pxRelevant);
					Keyframe[] pyFix = SimplifyAndLinearize(posy, .001f, out pyRelevant);
					Keyframe[] pzFix = SimplifyAndLinearize(posz, .001f, out pzRelevant);
					if (pxRelevant || pyRelevant || pzRelevant)
					{
						AnimationCurve curvepx = new AnimationCurve(pxFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curvepy = new AnimationCurve(pyFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curvepz = new AnimationCurve(pzFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalPosition.x", curvepx);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalPosition.y", curvepy);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalPosition.z", curvepz);
					}
					bool rxRelevant, ryRelevant, rzRelevant, rwRelevant;
					Keyframe[] rxFix = SimplifyAndLinearize(rotx, 0.002f, out rxRelevant);
					Keyframe[] ryFix = SimplifyAndLinearize(roty, 0.002f, out ryRelevant);
					Keyframe[] rzFix = SimplifyAndLinearize(rotz, 0.002f, out rzRelevant);
					Keyframe[] rwFix = SimplifyAndLinearize(rotw, 0.002f, out rwRelevant);
					if (rxRelevant || ryRelevant || rzRelevant || rwRelevant)
					{
						AnimationCurve curverx = new AnimationCurve(rxFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curvery = new AnimationCurve(ryFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curverz = new AnimationCurve(rzFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curverw = new AnimationCurve(rwFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalRotation.x", curverx);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalRotation.y", curvery);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalRotation.z", curverz);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalRotation.w", curverw);
					}
					bool sxRelevant, syRelevant, szRelevant;
					Keyframe[] sxFix = SimplifyAndLinearize(scalex, 0.01f, out sxRelevant);
					Keyframe[] syFix = SimplifyAndLinearize(scaley, 0.01f, out syRelevant);
					Keyframe[] szFix = SimplifyAndLinearize(scalez, 0.01f, out szRelevant);
					if (sxRelevant || syRelevant || szRelevant)
					{
						AnimationCurve curvesx = new AnimationCurve(sxFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curvesy = new AnimationCurve(syFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						AnimationCurve curvesz = new AnimationCurve(szFix) {postWrapMode = WrapMode.Default, preWrapMode = WrapMode.Default};
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalScale.x", curvesx);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalScale.y", curvesy);
						ac.SetCurve(parameterPath, typeof(Transform), "m_LocalScale.z", curvesz);
					}
				}
			}

            animation.AddClip(ac, ac.name);
			if (animation.clip == null)
				animation.clip = ac;

			return ac;
		}

		private static Keyframe[] SimplifyAndLinearize(Keyframe[] keys, float thresh, out bool relevant)
		{
			relevant = false;

			if (keys.Length == 0)
				return null;
			if (keys.Length == 1)
			{
				relevant = true;
				return keys;
			}

			List<Keyframe> taken = new List<Keyframe>();
			taken.Add(new Keyframe(keys[0].time, keys[0].value, 0f, (keys[1].value-keys[0].value)/(keys[1].time-keys[0].time)));
			Keyframe cur = taken[0];
			int lastTaken = 0;
			for (int i = 1; i < keys.Length; i++)
			{
				float t = keys[i].time;
				float vAtT = cur.value + cur.outTangent * (t - cur.time);

				if (i < keys.Length - 1 && Collab.Base.Math.Helpers.Matches(vAtT, keys[i].value, thresh))
					continue;

				relevant = true;
	
				if (lastTaken < i - 1)
				{
					int k = i - 1;
					float outTan = k < keys.Length - 1 
						? (keys[k + 1].value - keys[k].value) / (keys[k + 1].time - keys[k].time) 
						: 0f;
					Keyframe n = new Keyframe(keys[k].time, keys[k].value, cur.outTangent, outTan);
					taken.Add(n);
					cur = n;
				}
				{
					float outTan = i < keys.Length - 1
						? (keys[i + 1].value - keys[i].value) / (keys[i + 1].time - keys[i].time)
						: 0f;
					Keyframe n = new Keyframe(t, keys[i].value, cur.outTangent, outTan);
					taken.Add(n);
					cur = n;
				}

				lastTaken = i;
			}

			if (taken.Count == 2 && Mathf.Approximately(taken[0].value, taken[1].value))
				relevant = false;

			return taken.ToArray();
		}

		private static string ResolveAnimationPathTo(GameObject container, object modifierComponent)
		{
			if (modifierComponent is IHoloLoadedComponentConsideredAMaterial meh)
			{
				return ResolveAnimationPathTo(container, meh.FindUsageOfMaterial());
			}
			if (modifierComponent is Material mat)
			{
				var foundRenderer = container.GetComponentsInChildren<Renderer>().FirstOrDefault(r => Enumerable.Contains(r.sharedMaterials, mat));
				if (foundRenderer == null)
				{
					DebugOutput.Info("Animated material not found on any renderers, animation will be lost.");
				}
				else
				{
					return ResolveAnimationPathTo(container, foundRenderer.gameObject);
				}
			}

			string curPath = "";
			GameObject targetObject = (modifierComponent as GameObject) ?? (modifierComponent as Component)?.gameObject;
			while (targetObject != container)
			{
				curPath = targetObject.name + (curPath == "" ? "" : ("/" + curPath));
				targetObject = (targetObject as GameObject).transform.parent.gameObject;
			}
			return curPath;
		}
	}
}
