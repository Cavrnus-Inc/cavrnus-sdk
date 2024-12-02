using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Collab.Holo;
using Newtonsoft.Json;
using UnityBase.Content.DefaultHoloComponents;
using UnityEngine;

namespace UnityBase.Content
{
	public class InteractionScriptAdvancedComponent : HoloComponent
	{
		public enum ContinuationTriggerEnum
		{
			Proximity,
			Interact,
			Time,
			None
		}

		[Serializable]
		public class InteractStep
		{
			public AnimationClip[] anims;
			public AudioClip[] audios;
			public ParticleSystem[] particles;
			public string[] otherPlaybackProps;

			public AnimationClip[] doNotStopAnims;
			public AudioClip[] doNotStopAudios;
			public ParticleSystem[] doNotStopParticles;
			public string[] doNotStopOtherPlaybackProps;

			public ContinuationTriggerEnum nextStepTrigger;
			public float proximityDistance;
			public float waitTime;

			public float blendTime = .4f;
			public float blendEase = 1f;

			public float playbackTimeStart = 0f;
			public float playbackTimeSpeed = 1f;
			
			public bool loopAnimation = true;

			public int nextStep = -1;
		}

		private class InteractStepJs
		{
			public List<string> playbackProperties = new List<string>();
			public List<string> enabledProperties = new List<string>();
			public List<string> weightProperties = new List<string>();
			public List<string> stopPlaybackProperties = new List<string>();
			public List<string> disableProperties = new List<string>();
			public List<string> deWeightProperties = new List<string>();
			public string nextTriggerType = "none";
			public float? proximityDistance;
			public float? waitTime;
			public float? blendTime;
			public float? blendEase;
			public float? playbackTimeStart;
			public float? playbackTimeSpeed;
			public int? nextInd;
		}

		public InteractStep[] steps = new InteractStep[1];
		public string uniqueSequenceId;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo conversions)
		{
			var hsb = t.CreateNewComponent<ScriptHoloComponent>(parent);
			conversions.AfterAnimations(() =>
			{
				InteractStepJs[] js = new InteractStepJs[steps.Length];

				// First build the enables
				for (int i = 0; i < steps.Length; i++)
				{
					js[i] = new InteractStepJs();

					ObjectsToPropertyNames(t, parent, conversions, steps[i].anims?.ToList(), steps[i].audios?.ToList(), steps[i].particles?.ToList(), steps[i].otherPlaybackProps?.ToList(),
						out var playbackProps, out var weightProps, out var enableProps);
					js[i].playbackProperties.AddRange(playbackProps);
					js[i].weightProperties.AddRange(weightProps);
					js[i].enabledProperties.AddRange(enableProps);

					js[i].nextTriggerType = steps[i].nextStepTrigger.ToString().ToLowerInvariant();
					js[i].proximityDistance = steps[i].proximityDistance;
					js[i].waitTime = steps[i].waitTime;
					js[i].blendTime = steps[i].blendTime;
					js[i].blendEase = steps[i].blendEase;
					js[i].playbackTimeSpeed = steps[i].playbackTimeSpeed;
					js[i].playbackTimeStart = steps[i].playbackTimeStart;

					if (steps[i].nextStep >= 0)
						js[i].nextInd = steps[i].nextStep;
				}

				// Then accrue the disables, except those set as not-disabled
				for (int i = 0; i < steps.Length; i++)
				{
					// Figure out the tagged entries first
					ObjectsToPropertyNames(t, parent, conversions, steps[i].doNotStopAnims?.ToList(), steps[i].doNotStopAudios?.ToList(), steps[i].doNotStopParticles?.ToList(), steps[i].doNotStopOtherPlaybackProps?.ToList(),
						out var doNotStopPlaybackProps, out var doNotStopWeightProps, out var doNotStopEnableProps);

					for (int k = 0; k < steps.Length; k++)
					{
						if (k == i)
							continue;
						//if (steps[k].nextStep != i && (k + 1) % steps.Length != i) // Only proceed if step 'i' follows 'k'.
						//	continue;

						// Add k's enables to i's disables, unless otherwise tagged
						js[i].disableProperties.AddRange(js[k].enabledProperties.Except(doNotStopEnableProps).Except(js[i].enabledProperties));
						js[i].deWeightProperties.AddRange(js[k].weightProperties.Except(doNotStopWeightProps).Except(js[i].weightProperties));
						js[i].stopPlaybackProperties.AddRange(js[k].playbackProperties.Except(doNotStopPlaybackProps).Except(js[i].playbackProperties));
					}
				}


				StringBuilder d = new StringBuilder();
				d.Append("[");
				var jsjson = String.Join(",\n",js.Select(jsv=>JsonConvert.SerializeObject(jsv, Formatting.Indented, new JsonSerializerSettings(){NullValueHandling = NullValueHandling.Ignore})));
				d.Append(jsjson);
				d.Append("]");

				string dataText = d.ToString();
				string uniqueid = String.IsNullOrWhiteSpace(uniqueSequenceId) ? "'sequence'" : $"'{uniqueSequenceId}'";
				string finalText = InteractionScriptTemplateSource.Replace("{<stepsdata>}", dataText).Replace("{<uniqueseqid>}", uniqueid);
				hsb.Data = new ScriptData() { Script = finalText, ScriptVersion = Version.Parse("0.2") };
			});
			return hsb.ComponentId;
		}

		private void ObjectsToPropertyNames(HoloRoot t, HoloNode parent, IUnityToHolo conversions, 
			List<AnimationClip> anims, List<AudioClip> audios, List<ParticleSystem> particles, List<string> extras,
			out List<string> playbackProps, out List<string> weightProps, out List<string> enableProps)
		{
			var animpaths = new List<string>();
			foreach (var animclip in anims)
			{
				var foundid = conversions.SearchForOrConvertComponent(animclip, this.gameObject, t, parent);
				if (foundid != null)
				{
					animpaths.Add(foundid.ComponentId.ToString());
				}
			}

			playbackProps = new List<string>();
			weightProps = new List<string>();
			enableProps = new List<string>();
			playbackProps.AddRange(extras.Select(pp => $"../{pp}"));

			if (audios != null)
			{
				for (int j = 0; j < audios.Count; j++)
				{
					var foundaudioid = conversions.SearchForOrConvertComponent(audios[j], this.gameObject, t, parent);
					if (foundaudioid != null)
					{
						playbackProps.Add($"../{foundaudioid.ComponentId}/playbackTime");
						// Can blend in audio volumes here by uncommenting:
						// weightProps.Add($"../{foundaudioid.ComponentId}/volume");
					}
				}
			}
			if (particles != null)
			{
				for (int j = 0; j < particles.Count; j++)
				{
					var foundparticleid = conversions.SearchForOrConvertComponent(particles[j], this.gameObject, t, parent);
					if (foundparticleid != null)
						playbackProps.Add($"../{foundparticleid.ComponentId}/playbackTime");
				}
			}

			for (int j = 0; j < animpaths.Count; j++)
			{
				playbackProps.Add($"../{animpaths[j]}/playbackTime");
				enableProps.Add($"../{animpaths[j]}/animationEnabledState");
				weightProps.Add($"../{animpaths[j]}/animationWeight");
			}
		}

		private const string InteractionScriptTemplateSource = @"

import cav from '@cavrnus/runtime';
const data = {<stepsdata>};
const uniqueSequenceId = {<uniqueseqid>};
const sequenceId = 'seq-' + uniqueSequenceId;
// Set up step number property
cav.prop.declareScalarProperty(sequenceId, { default: 0, meta: { base: { name: `Playback Sequence ${uniqueSequenceId}`, description: 'State Ordinal For Playback Sequence', category: 'Hidden' } } });
for (let ind = 0; ind < data.length; ind++) {
    let nextInd = (ind + 1) % data.length;
    if (data[ind].nextInd !== undefined && data[ind].nextInd >= 0)
        nextInd = (data[ind].nextInd) % data.length;
    // Set up triggers between steps
    switch (data[ind].nextTriggerType) {
        case 'interact':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .interaction({ interactionType: 'interact' }, cav.data.view.contextual().asPart().root())
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, nextInd);
                ops.commit();
            });
            break;
        case 'time':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .timer({ delay: data[ind].waitTime ?? 1, repeatDelay: data[ind].waitTime ?? 1 })
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, nextInd);
                ops.commit();
            });
            break;
        case 'proximity':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .intersection({ mode: 'anyintersection', requiredTime: .2 }, cav.data.view.contextual().asPart().root(), { center: { x: 0, y: 0, z: 0 }, radius: data[ind].proximityDistance ?? 2.0 }, cav.data.view.localUser(), undefined)
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, nextInd);
                ops.commit();
            });
            break;
    }
}
// Now set up invocations for each step index to apply them.
// Stop playback and enables for not-our-index, unless it is also in our-index.
// Start playbacks and enables for our index
cav.on({ synced: false })
    .valueScalar(sequenceId, true)
    .then(v => {
    let ind = Math.round(v.value);
    if (ind < 0)
        ind = 0;
    if (ind >= data.length)
        ind = data.length - 1;
    // make it 'ind'.
    const ops = cav.beginLocalOperations();
    const newLiveStep = data[ind];
    // Now activate live step
    // First the disables
    if (newLiveStep.disableProperties !== undefined) {
        for (const enabledProp of newLiveStep.disableProperties) {
            ops.prop.updateBoolean(enabledProp, false);
        }
    }
    if (newLiveStep.deWeightProperties !== undefined) {
        for (const weightProp of newLiveStep.deWeightProperties) {
            ops.prop.updateScalarInterpolate(weightProp, 0.0, { duration: newLiveStep.blendTime ?? 0, easeIn: newLiveStep.blendEase ?? 0, easeOut: newLiveStep.blendEase ?? 0 });
        }
    }
    if (newLiveStep.stopPlaybackProperties !== undefined) {
        for (const playbackProp of newLiveStep.stopPlaybackProperties) {
            ops.prop.updatePlaybackPause(playbackProp);
        }
    }
    // Then the enables
    if (newLiveStep.enabledProperties !== undefined) {
        for (const enabledProp of newLiveStep.enabledProperties) {
            ops.prop.updateBoolean(enabledProp, true);
        }
    }
    if (newLiveStep.weightProperties !== undefined) {
        for (const weightProp of newLiveStep.weightProperties) {
            ops.prop.updateScalarInterpolate(weightProp, 1.0, { duration: newLiveStep.blendTime ?? 0, easeIn: newLiveStep.blendEase ?? 0, easeOut: newLiveStep.blendEase ?? 0 });
        }
    }
    if (newLiveStep.playbackProperties !== undefined) {
        for (const playbackProp of newLiveStep.playbackProperties) {
            ops.prop.updateScalarGen(playbackProp, { playbackBasic: { playing: true, startTime: newLiveStep.playbackTimeStart ?? 0.0, speedWhilePlaying: newLiveStep.playbackTimeSpeed ?? 1.0 } });
        }
    }
    // no commit; unsynced changes; the index assignment is synced so no further sync should be needed.
});

";
	}
}