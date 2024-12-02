using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.Content
{
	[Serializable]
	public class MetaDataEntry
	{
		public string Key;
		public string Value;
	}

	public class MetaDataComponent : MonoBehaviour
	{
		public List<MetaDataEntry> MetaData = new List<MetaDataEntry>();
	}
}