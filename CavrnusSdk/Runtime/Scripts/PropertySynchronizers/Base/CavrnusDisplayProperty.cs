using System;
using System.Collections.Generic;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusSdk.PropertySynchronizers
{
	public class CavrnusDisplayProperty<T> : IDisposable
	{
		private List<IDisposable> disposables = new List<IDisposable>();

		private ICavrnusValueSync<T> sync;

		public CavrnusDisplayProperty(ICavrnusValueSync<T> sync)
		{
			this.sync = sync;

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection);
		}

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			if (typeof(T) == typeof(Color)) 
			{
                spaceConn.DefineColorPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (Color)(object)sync.GetValue());

                var bndDisp = spaceConn.BindColorPropertyValue(sync.Context.UniqueContainerPath, sync.PropName, (data) => sync.SetValue((T)(object)data));
                disposables.Add(bndDisp);
            }
			else if (typeof(T) == typeof(string)) 
			{
                spaceConn.DefineStringPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (string)(object)sync.GetValue());

                var bndDisp = spaceConn.BindStringPropertyValue(sync.Context.UniqueContainerPath, sync.PropName, (data) => sync.SetValue((T)(object)data));
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(bool)) 
			{
                spaceConn.DefineBoolPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (bool)(object)sync.GetValue());

                var bndDisp = spaceConn.BindBoolPropertyValue(sync.Context.UniqueContainerPath, sync.PropName, (data) => sync.SetValue((T)(object)data));
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(Vector4)) 
			{
                spaceConn.DefineVectorPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (Vector4)(object)sync.GetValue());

                var bndDisp = spaceConn.BindVectorPropertyValue(sync.Context.UniqueContainerPath, sync.PropName, (data) => sync.SetValue((T)(object)data));
                disposables.Add(bndDisp);
            }
			else if (typeof(T) == typeof(float)) 
			{
                spaceConn.DefineFloatPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (float)(object)sync.GetValue());

                var bndDisp = spaceConn.BindFloatPropertyValue(sync.Context.UniqueContainerPath, sync.PropName, (data) => sync.SetValue((T)(object)data));
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(CavrnusTransformData)) 
			{
                spaceConn.DefineTransformPropertyDefaultValue(sync.Context.UniqueContainerPath, sync.PropName, (CavrnusTransformData)(object)sync.GetValue());

                var bndDisp = spaceConn.BindTransformPropertyValue(sync.Context.UniqueContainerPath, sync.PropName,	(data) => sync.SetValue((T) (object)data));
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