using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content
{
	// Required because UnityEditor is required to pull out animation data.
	public class AnimationClipsRecordComponent : MonoBehaviour
	{
		public class AnimationSequenceRecord
		{
			public List<AnimationKeyframeSingleData> Keyframes;
			public GameObject targetObject;
			public string targetProperty;
			public Type targetComponentType;
		}
		public class AnimationClipRecord
		{
			public string Name { get; set; } = "";
			public AnimationSequenceBoundaryBehaviorEnum BoundaryBehavior { get; set; } = AnimationSequenceBoundaryBehaviorEnum.Clamp;
			public List<AnimationSequenceRecord> KeyframeSequences = new List<AnimationSequenceRecord>();
		}

		public List<AnimationClipRecord> ClipRecords { get; set; } = new List<AnimationClipRecord>();
	}
}
