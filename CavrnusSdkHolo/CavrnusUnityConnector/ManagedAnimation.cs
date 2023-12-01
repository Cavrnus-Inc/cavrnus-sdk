using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Holo;
using Collab.LiveRoomSystem.GameEngineConnector;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ContainerContext;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.ScalarProp;
using Collab.Proxy.Prop.TransformProp;
using UnityBase.Content;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace CavrnusSdk
{
	internal class ManagedAnimation : IManagedEngineObject, IDisposedElement
	{
		public PropertyId Id { get; private set; }

		private string[] AnimatedTransformPropertyPaths;
		public WrappedPlaybackDataModel PlaybackState;
		private PropertySetManager container;
		private Animation anim;

		public string ClipName;

		public ManagedAnimation(PropertySetManager container, IManagedEngineObject parent, CreateAnimationClipRequest req, Dictionary<PropertyId, IManagedEngineObject> idsLookup)
		{
			ClipName = req.HoloAnimReq.Name;

			this.container = container;
			Id = container.AbsoluteId;

			AnimationConvert.ConvertToAnimationClip(req.HoloAnimReq, (parent as ManagedTransform).Trans.gameObject,
				(hci) =>
				{
					var id = container.AbsoluteId.Pop().Push(hci.ComponentId.ToString());
					if (!idsLookup.ContainsKey(id))
					{
						DebugOutput.Error($"Failed to find key with ID {id}");
						return null;
					}

					var ob = idsLookup[id];
					if (ob is ManagedGameObject mgo)
						return mgo.Ob;
					else if (ob is ManagedTransform mt)
						return mt.Trans;
					else if (ob is ManagedMaterial mm)
						return mm.Mat;
					else
						return null;
				});
	

			var animatedMatrixIds = req.HoloAnimReq.MatrixSequences?.Select(m => m.TargetComponent) ?? Enumerable.Empty<HoloComponentIdentifier>();
			var animatedSingleAnimmedTransforms = req.HoloAnimReq.SingleSequences
				                                      ?.Where(ss => ss.TargetParameter.ParameterName.Contains("_LocalPosition") || ss.TargetParameter.ParameterName.Contains("_LocalRotation") || ss.TargetParameter.ParameterName.Contains("_LocalScale"))
				                                      ?.Select(ss => ss.TargetParameter.TargetComponent)
			                                      ?? Enumerable.Empty<HoloComponentIdentifier>();
			var combinedUniques = animatedMatrixIds.Concat(animatedSingleAnimmedTransforms).Distinct();
			var animTransformsAsProperties = combinedUniques.Select(aa => $"../{aa.ComponentId.ToString()}/{PropertyDefs.Objects_Transform}");
			AnimatedTransformPropertyPaths = animTransformsAsProperties.ToArray();

			anim = (parent as ManagedTransform).Trans.GetComponent<Animation>();

			PlaybackState = new WrappedPlaybackDataModel(container, PropertyDefs.Objects_PlaybackTime,
				PropertyDefs.Objects_PlaybackLength, PropertyDefs.Objects_PlaybackLoop);

			SetupBindings(container, anim);
		}

		public IReadonlySetting<bool> clipEnabled;
		public IReadonlySetting<double> clipWeight;

		private void SetupBindings(PropertySetManager container, Animation anim)
		{
			clipEnabled = container.GetBooleanProperty(PropertyDefs.Objects_AnimationEnabledState).Current.Translating(c => c.Value);
			clipWeight = container.GetScalarProperty(PropertyDefs.Objects_AnimationWeight).Current.Translating(c => c.Value);

			var animEnabledBnd = container.GetBooleanProperty(PropertyDefs.Objects_AnimationEnabledState)
				.Bind(en => SetPlaybackState(en, PlaybackState.Playing.Value, PlaybackState.EvaluatedPlayTime.Value, PlaybackState.Speed.Value, clipWeight.Value));
			animEnabledBnd.DisposeOnDestroy(this);

			var animWeightBnd = container.GetScalarProperty(PropertyDefs.Objects_AnimationWeight)
				.Bind(nweight => SetPlaybackState(clipEnabled.Value, PlaybackState.Playing.Value, PlaybackState.EvaluatedPlayTime.Value, PlaybackState.Speed.Value, nweight));
			animEnabledBnd.DisposeOnDestroy(this);

			var partClearBnd = container.GetBooleanProperty(PropertyDefs.Objects_AnimationEnabledState)
				.Bind(en => EnabledChanged(en));
			partClearBnd.DisposeOnDestroy(this);

			PlaybackState.EvaluatedPlayTime.ChangedEvent += PlayTimeOnChangedEvent;
			PlaybackState.Playing.ChangedEvent += IsPlayingOnChangedEvent;
			PlaybackState.Speed.ChangedEvent += SpeedWhilePlayingOnChangedEvent;
		}


		private void IsPlayingOnChangedEvent(bool newvalue, bool oldvalue)
		{
			SetPlaybackState(clipEnabled.Value, newvalue, PlaybackState.EvaluatedPlayTime.Value, PlaybackState.Speed.Value, clipWeight.Value);
		}

		private void PlayTimeOnChangedEvent(double newvalue, double oldvalue)
		{
			SetPlaybackState(clipEnabled.Value, PlaybackState.Playing.Value, newvalue, PlaybackState.Speed.Value, clipWeight.Value);
		}

		private void SpeedWhilePlayingOnChangedEvent(double newvalue, double oldvalue)
		{
			SetPlaybackState(clipEnabled.Value, PlaybackState.Playing.Value, PlaybackState.EvaluatedPlayTime.Value, newvalue, clipWeight.Value);

		}

		private bool currPlaying = false;
		private void SetPlaybackState(bool clipEnabled, bool playing, double time, double speed, double weight)
		{
			if (currPlaying != playing)
				SetPartsPlayState(playing);
			currPlaying = playing;

			anim[ClipName].enabled = clipEnabled && weight > 0f;
			anim[ClipName].weight = clipEnabled ? (float)weight : 0f;
			anim[ClipName].layer = anim.gameObject.GetComponentInChildren<AnimationClipLayerRecord>()?.Get(ClipName) ?? 0;
			anim[ClipName].speed = 0;
			anim[ClipName].time = (float)time;
		}


		private bool animOverridesAssigned = false;
		private void SetPartsPlayState(bool playing)
		{
			PreAnimReset();

			if (AnimatedTransformPropertyPaths != null)
			{
				foreach (var atpp in AnimatedTransformPropertyPaths)
				{
					container.SearchForTransformProperty(new PropertyId(atpp)).UpdateValue("anim", playing ? 1 : 0, new TransformSetGeneratorUseSource());
				}
			}
			animOverridesAssigned = true;
		}

		private void EnabledChanged(bool en)
		{
			if (en)
			{
				SetPartsPlayState(PlaybackState.Playing.Value);
			}
			else if (animOverridesAssigned)
			{
				if (AnimatedTransformPropertyPaths != null)
				{
					foreach (var atpp in AnimatedTransformPropertyPaths)
					{
						container.SearchForTransformProperty(new PropertyId(atpp)).ClearValue("anim");
					}
				}
				animOverridesAssigned = false;
			}

		}

		private void PreAnimReset()
		{
			if (AnimatedTransformPropertyPaths != null)
			{
				foreach (var atpp in AnimatedTransformPropertyPaths)
				{
					var psm = container.SearchForContainerForProperty(new PropertyId(atpp));
					if (psm != null)
					{
						var partContext = psm.GetFirstContextOfType<HoloPartPropertyContainerContext>();
						if (partContext?.Value is HoloPartPropertyContainerContext hppcc)
						{
							/*var actualObject = Singletons.SessionObjectTracker().FindGameObjectWithId(hppcc.ObjectId);

							if (actualObject != null)
							{
								var posData = actualObject.GetComponent<PartDataContainer>().Get<OriginalPositionData>();
								actualObject.transform.localPosition = posData.LocalSnapbackPos;
								actualObject.transform.localEulerAngles = posData.LocalSnapbackRot;
							}*/
						}
					}
				}
			}
		}

		public void Dispose() => Disposed?.Invoke();
		public event Action Disposed;
	}

}
