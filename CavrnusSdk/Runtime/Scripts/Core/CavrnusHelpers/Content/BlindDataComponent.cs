using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content
{
	public class BlindDataComponent : MonoBehaviour
	{
		public HoloTypeIdentifier HoloType { get; set; }
		public byte[] BlindData { get; set; }
	}
}