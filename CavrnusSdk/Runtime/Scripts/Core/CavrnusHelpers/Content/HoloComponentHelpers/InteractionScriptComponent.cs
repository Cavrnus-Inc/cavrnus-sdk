using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Collab.Holo;
using UnityBase.Content.DefaultHoloComponents;
using UnityEngine;

namespace UnityBase.Content
{
	public class InteractionScriptComponent : HoloComponent
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

			public ContinuationTriggerEnum nextStepTrigger;
			public float proximityDistance;
			public float waitTime;

			public float blendTime = .4f;
			public float blendEase = 1f;

			public float playbackTimeStart = 0f;
			public float playbackTimeSpeed = 1f;
			
			public bool loopAnimation = true;
		}

		public InteractStep[] steps = new InteractStep[1];
		public string uniqueSequenceId;

		public override HoloComponentIdentifier BuildComponent(HoloRoot t, HoloNode parent, IUnityToHolo conversions)
		{
			var hsb = t.CreateNewComponent<ScriptHoloComponent>(parent);
			conversions.AfterAnimations(() =>
			{
				StringBuilder d = new StringBuilder();
				d.Append("[");
				for (int i = 0; i < steps.Length; i++)
				{
					var animpaths = new List<string>();
					foreach (var animclip in steps[i].anims)
					{
						var foundid = conversions.SearchForOrConvertComponent(animclip, this.gameObject, t, parent);
						if (foundid != null)
						{
							animpaths.Add(foundid.ComponentId.ToString());
						}
					}

					d.Append("{playbackProperties:[");
					for (int j = 0; j < steps[i].otherPlaybackProps.Length; j++)
					{
						d.Append($"'../{steps[i].otherPlaybackProps[j]}',");
					}

					if (steps[i].audios != null)
					{
						for (int j = 0; j < steps[i].audios.Length; j++)
						{
							var foundaudioid = conversions.SearchForOrConvertComponent(steps[i].audios[j], this.gameObject, t, parent);
							if (foundaudioid != null)
								d.Append($"'../{foundaudioid.ComponentId}/playbackTime',");
						}
					}
					if (steps[i].particles != null)
					{
						for (int j = 0; j < steps[i].particles.Length; j++)
						{
							var foundparticleid = conversions.SearchForOrConvertComponent(steps[i].particles[j], this.gameObject, t, parent);
							if (foundparticleid != null)
								d.Append($"'../{foundparticleid.ComponentId}/playbackTime',");
						}
					}

					for (int j = 0; j < animpaths.Count; j++)
					{
						d.Append($"'../{animpaths[j]}/playbackTime',");
					}

					d.Append("],enabledProperties:[");
					for (int j = 0; j < animpaths.Count; j++)
					{
						d.Append($"'../{animpaths[j]}/animationEnabledState',");
					}

					d.Append("],weightProperties:[");
					for (int j = 0; j < animpaths.Count; j++)
					{
						d.Append($"'../{animpaths[j]}/animationWeight',");
					}

					d.Append($"],nextTriggerType:'{steps[i].nextStepTrigger.ToString().ToLowerInvariant()}',proximityDistance:{steps[i].proximityDistance},waitTime:{steps[i].waitTime},blendTime:{steps[i].blendTime},blendEase:{steps[i].blendEase},playbackTimeStart:{steps[i].playbackTimeStart},playbackTimeSpeed:{steps[i].playbackTimeSpeed}}}");
					if (i < steps.Length - 1)
						d.Append(",");
				}

				d.Append("]");

				string dataText = d.ToString();
				string uniqueid = String.IsNullOrWhiteSpace(uniqueSequenceId) ? "'sequence'" : $"'{uniqueSequenceId}'";
				string finalText = InteractionScriptTemplateSource.Replace("{<stepsdata>}", dataText).Replace("{<uniqueseqid>}", uniqueid);
				hsb.Data = new ScriptData() { Script = finalText, ScriptVersion = Version.Parse("0.2") };
			});
			return hsb.ComponentId;
		}

		private const string InteractionScriptTemplateSource = @"

import cav from '@cavrnus/runtime';

const data = {<stepsdata>};
const uniqueSequenceId = {<uniqueseqid>};
const sequenceId = 'seq-' + uniqueSequenceId;
// Set up step number property
cav.prop.declareScalarProperty(sequenceId, { default: 0, meta: { base: { name: `Playback Sequence ${uniqueSequenceId}`, description: 'State Ordinal For Playback Sequence', category: 'Hidden' } } });
for (let ind = 0; ind < data.length; ind++) {
    // Set up triggers between steps
    switch (data[ind].nextTriggerType) {
        case 'interact':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .interaction({ interactionType: 'interact' }, cav.data.view.contextual().asPart().root())
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, (ind >= data.length - 1) ? 0 : (ind + 1));
                ops.commit();
            });
            break;
        case 'time':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .timer({ delay: data[ind].waitTime ?? 1, repeatDelay: data[ind].waitTime ?? 1 })
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, (ind >= data.length - 1) ? 0 : (ind + 1));
                ops.commit();
            });
            break;
        case 'proximity':
            cav.on({ synced: true, activeWhileExpr: `ref('${sequenceId}') == ${ind}` }, `syncid-${sequenceId}-${ind}`)
                .intersection({ mode: 'anyintersection', requiredTime: .2 }, cav.data.view.contextual().asPart().root(), { center: { x: 0, y: 0, z: 0 }, radius: data[ind].proximityDistance ?? 2.0 }, cav.data.view.localUser(), undefined)
                .then(v => {
                var ops = cav.beginOperations({ sendTransients: false, undoable: false });
                ops.prop.updateScalar(sequenceId, (ind >= data.length - 1) ? 0 : (ind + 1));
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
    const ops = cav.beginOperations({ sendTransients: false, undoable: false }); // no transients and no commit; local application only.
    const newLiveStep = data[ind];
    for (let iter = 0; iter < data.length; iter++) {
        if (iter === ind) // skip where we're assigning for now; we'll make it active after deactivating the others.
            continue;
        const step = data[iter];
        if (step.enabledProperties !== undefined) {
            for (const enabledProp of step.enabledProperties) {
                if (newLiveStep.enabledProperties === undefined || !(newLiveStep.enabledProperties.includes(enabledProp))) ;
            }
        }
        if (step.weightProperties !== undefined) {
            for (const weightProp of step.weightProperties) {
                if (newLiveStep.weightProperties === undefined || !(newLiveStep.weightProperties.includes(weightProp))) {
                    ops.prop.updateScalarInterpolate(weightProp, 0.0, { duration: newLiveStep.blendTime ?? 0, easeIn: newLiveStep.blendEase ?? 0, easeOut: newLiveStep.blendEase ?? 0 });
                }
            }
        }
        if (step.playbackProperties !== undefined) {
            for (const playbackProp of step.playbackProperties) {
                if (newLiveStep.playbackProperties === undefined || !(newLiveStep.playbackProperties.includes(playbackProp))) {
                    // Disable it only if not enabling it in the new state
                    ops.prop.updatePlaybackPause(playbackProp);
                }
            }
        }
    }
    // Now activate live step
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