using System.Collections.Generic;
using Collab.Holo;
using UnityEngine;

namespace UnityBase.Content
{
	// Required because UnityEditor is required to extract shader information.
	public class CustomMaterialRecordComponent : MonoBehaviour
	{
		public class CustomScalarRecord
		{
			public string parameterName;
			public float minimum;
			public float maximum;
		}

		public List<CustomScalarRecord> scalarParameters = new List<CustomScalarRecord>();
		public List<string> float3Parameters = new List<string>();
		public List<string> colorParameters = new List<string>();
		public List<string> imageParameters = new List<string>();

		public List<CustomScalarRecord> ScalarParameters
		{
			get => scalarParameters;
			set => scalarParameters = value;
		}

		public List<string> Float3Parameters
		{
			get => float3Parameters;
			set => float3Parameters = value;
		}

		public List<string> ColorParameters
		{
			get => colorParameters;
			set => colorParameters = value;
		}
		public List<string> ImageParameters
		{
			get => imageParameters;
			set => imageParameters = value;
		}
	}
}