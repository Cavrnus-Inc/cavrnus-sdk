using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusDisplayProperty<T> : IDisposable
	{
		private List<IDisposable> disposables = new List<IDisposable>();

		private ICavrnusValueSync<T> sync;

		public CavrnusDisplayProperty(ICavrnusValueSync<T> sync)
		{
			this.sync = sync;

			CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection);
		}

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			if (typeof(T) == typeof(Color)) {
				if (!CavrnusPropertyHelpers.ColorPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                               sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineColorPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(Color) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToColorProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName, v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(string)) {
				if (!CavrnusPropertyHelpers.StringPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                                sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineStringPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(string) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToStringProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName, v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(string)) {
				if (!CavrnusPropertyHelpers.StringPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                                sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineStringPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(string) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToStringProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName, v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(bool)) {
				if (!CavrnusPropertyHelpers.BooleanPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                              sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineBooleanPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(bool) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToBooleanProperty(spaceConn, sync.Context.UniqueContainerPath,
				                                                        sync.PropName,
				                                                        v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(Vector4)) {
				if (!CavrnusPropertyHelpers.VectorPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                                sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineVectorPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(Vector4) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToVectorProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName, v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(float)) {
				if (!CavrnusPropertyHelpers.FloatPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                               sync.PropName)) {
					var defDisp = CavrnusPropertyHelpers.DefineFloatPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
						(float) (object) sync.GetValue());
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToFloatProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName, v => sync.SetValue((T) (object) v));
				disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(CavrnusPropertyHelpers.TransformData)) {
				if (!CavrnusPropertyHelpers.TransformPropertyHasDefaultValue(spaceConn, sync.Context.UniqueContainerPath,
				                                                   sync.PropName)) {
					var v = (CavrnusPropertyHelpers.TransformData) (object) sync.GetValue();
					var defDisp = CavrnusPropertyHelpers.DefineTransformPropertyDefaultValue(
						spaceConn, sync.Context.UniqueContainerPath, sync.PropName, 
						v.LocalPosition, v.LocalEulerAngles, v.LocalScale);
					disposables.Add(defDisp);
				}

				var bndDisp = CavrnusPropertyHelpers.BindToTransformProperty(
					spaceConn, sync.Context.UniqueContainerPath, sync.PropName,
					(p, r, s) => sync.SetValue((T) (object) new CavrnusPropertyHelpers.TransformData(p, r, s)));
				disposables.Add(bndDisp);
			}
			else {
				throw new Exception(
					$"Property value of type {typeof(T)} is not supported by CavrnusDisplayProperty yet!");
			}
		}

		public void Dispose()
		{
			//Dispose of everything
			foreach (var disp in disposables) disp.Dispose();
			disposables.Clear();
		}
	}
}