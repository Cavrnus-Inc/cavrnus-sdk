using System;
using UnityBase;
using UnityEngine;
using CavrnusSdk.API;

//Bringing over Core for convenient scheduler/wildcard transient access.  Think of it like bringing in any external tool.
using CavrnusCore;

namespace CavrnusSdk.PropertySynchronizers
{
	public class CavrnusPostSynchronizedProperty<T> : IDisposable
	{
		private CavrnusLivePropertyUpdate<T> updater = null;

		private CavrnusSpaceConnection spaceConn;

		private ICavrnusValueSync<T> sync;
		private IDisposable disp;
		const float pollingTime = .02f;

		public CavrnusPostSynchronizedProperty(ICavrnusValueSync<T> sync)
		{
			this.sync = sync;

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => spaceConn = sc);

			disp = CavrnusStatics.Scheduler.ExecInMainThreadRepeating(pollingTime, TryUpdate);
		}

		const float endChangeTimeGap = .3f;
		float lastChangeTime;

		private void TryUpdate()
		{
			//Don't do anything until we've loaded the space
			if (spaceConn == null) return;

			T currPropertyValue;
			if (typeof(T) == typeof(Color)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetColorPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(string)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetStringPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(bool)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetBoolPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(Vector4)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetVectorPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(float)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetFloatPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(CavrnusTransformData)) {
				currPropertyValue = (T) (object)CavrnusFunctionLibrary.GetTransformPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else {
				throw new Exception($"Property value of type {typeof(T)} is not supported by CavrnusPostSynchronizedProperty yet!");
			}

			T currUnityVal = sync.GetValue();
			bool hasChange = !currPropertyValue.Equals(currUnityVal);

			//Transforms can sometimes be a little wobbly w/ gravity etc.  Wanna give them a LITTLE bit of epsilon
			if (typeof(T) == typeof(CavrnusTransformData))
			{
				if(currPropertyValue is CavrnusTransformData td && currUnityVal is CavrnusTransformData utd)
				{
					hasChange = !td.LocalPosition.AlmostEquals(utd.LocalPosition, .0001f) || !td.LocalEulerAngles.AlmostEquals(utd.LocalEulerAngles, .0001f) || !td.LocalScale.AlmostEquals(utd.LocalScale, .0001f);
				}
			}

			if (!hasChange) {
				//This change has timed out, time to finalize it
				if (!sync.Context.IsUserProperty() && updater != null && Time.time - lastChangeTime > endChangeTimeGap) {
					updater.UpdateWithNewData(sync.GetValue());
					updater.Finish();
					updater = null;
				}

				//Our transform is synchronized w/ the server, so don't do any posting
				return;
			}

			lastChangeTime = Time.time;

			if (updater == null)
			{
                updater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName,
					sync.GetValue());
			}
			else 
			{ 
				updater.UpdateWithNewData(sync.GetValue()); 
			}
		}

		public void Dispose() { disp.Dispose(); }
	}
}