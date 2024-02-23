using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public class SyncMaterial : MonoBehaviour
	{
		public Material Material;

		public bool SendMyChanges = true;

		//private CavrnusDisplayProperty<T> displayer;
		//private CavrnusPostSynchronizedProperty<T> sender;

		private List<IDisposable> disps = new List<IDisposable>();

		private class MatVectorDisplayer : ICavrnusValueSync<Vector4>
		{
			private Material mat;
			private string propName;
			private CavrnusPropertiesContainer context;

			public MatVectorDisplayer(Material mat, string propName, CavrnusPropertiesContainer context)
			{
				this.propName = propName;
				this.mat = mat;
				this.context = context;
			}

			public string PropName => propName;
			public CavrnusPropertiesContainer Context => context;

			public Vector4 GetValue() { return mat.GetVector(propName); }

			public void SetValue(Vector4 value) { mat.SetVector(propName, value); }
		}

		private class MatScalerDisplayer : ICavrnusValueSync<float>
		{
			private Material mat;
			private string propName;
			private CavrnusPropertiesContainer context;
			private bool asInt;

			public MatScalerDisplayer(Material mat, string propName, CavrnusPropertiesContainer context, bool asInt)
			{
				Debug.Log("Syncing scalar " + propName);

				this.propName = propName;
				this.mat = mat;
				this.context = context;
				this.asInt = asInt;
			}

			public string PropName => propName;
			public CavrnusPropertiesContainer Context => context;

			public float GetValue()
			{
				if (asInt)
					return mat.GetInt(propName);
				else
					return mat.GetFloat(propName);
			}

			public void SetValue(float value)
			{
				if (asInt)
					mat.SetInt(propName, (int) value);
				else {
					Debug.Log("Setting float " + value + " to " + propName);
					mat.SetFloat(propName, value);
				}
			}
		}


		private void Start()
		{
			if (Material == null) throw new System.Exception($"No material selected for sync on object {name}");

			foreach (var vec in Material.GetPropertyNames(MaterialPropertyType.Vector)) {

				Func<bool> claimed = () => false;
				if (SendMyChanges)
				{
					var poster = new CavrnusPostSynchronizedProperty<Vector4>(new MatVectorDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>()));
					claimed = () => poster.transientUpdater != null;
					disps.Add(poster);
				}
				disps.Add(new CavrnusDisplayProperty<Vector4>(new MatVectorDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>()), claimed));
				
			}

			foreach (var vec in Material.GetPropertyNames(MaterialPropertyType.Float))
			{
				Func<bool> claimed = () => false;
				if (SendMyChanges)
				{
					var poster = new CavrnusPostSynchronizedProperty<float>(new MatScalerDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>(), false));
					claimed = () => poster.transientUpdater != null;
					disps.Add(poster);
				}
				disps.Add(new CavrnusDisplayProperty<float>(new MatScalerDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>(), false), claimed));
			}

			foreach (var vec in Material.GetPropertyNames(MaterialPropertyType.Int))
			{
				Func<bool> claimed = () => false;
				if (SendMyChanges)
				{
					var poster = new CavrnusPostSynchronizedProperty<float>(new MatScalerDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>(), true));
					claimed = () => poster.transientUpdater != null;
					disps.Add(poster);
				}
				disps.Add(new CavrnusDisplayProperty<float>(new MatScalerDisplayer(Material, vec, GetComponent<CavrnusPropertiesContainer>(), true), claimed));
			}
		}

		private void OnDestroy()
		{
			foreach (var disp in disps) disp.Dispose();
		}
	}
}