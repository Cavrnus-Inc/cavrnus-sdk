using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	[Serializable]
	public class CavrnusSettings : ScriptableObject
	{
		[Header(
			"Some devices don't support voice and video systems.  Disabling this will prevent build/runtime errors on them.")]
		public bool DisableVoiceAndVideo = false;

		[Header("Magic Leap builds require some special logic to work properly.")]
		public bool BuildingForMagicLeap;
	}
}