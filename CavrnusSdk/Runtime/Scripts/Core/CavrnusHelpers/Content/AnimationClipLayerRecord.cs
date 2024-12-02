using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBase.Content
{
	[Serializable]
	public class AnimationClipLayerRecord : UnityEngine.MonoBehaviour
	{
		[Serializable]
		public class LayerList
		{
			public List<string> names = new List<string>();
		}
		public List<LayerList> clipNamesByIndex = new List<LayerList>();

		public void Set(string clip, int layer)
		{
			while (clipNamesByIndex.Count <= layer)
				clipNamesByIndex.Add(new LayerList());
			clipNamesByIndex[layer].names.Add(clip);
		}
		public int Get(string clip)
		{
			for (int i = 0; i < clipNamesByIndex.Count; i++)
			{
				if (clipNamesByIndex[i].names.Contains(clip))
					return i;
			}
			return -1;
		}
	}
}
