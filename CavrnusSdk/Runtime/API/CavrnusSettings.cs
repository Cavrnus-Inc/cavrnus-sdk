using System;
using UnityEngine;

namespace CavrnusSdk
{
	[Serializable]
	public class CavrnusSettings : ScriptableObject
	{
		[Header("Some devices don't support voice and video systems. " + "Disabling this will prevent build/runtime errors on them.")]
		public bool DisableVoiceAndVideo = false;

		[Header("Some devices don't support AEC. " + "Disabling this will prevent build/runtime errors on them.")]
		public bool DisableAcousticEchoCancellation;
	}
}