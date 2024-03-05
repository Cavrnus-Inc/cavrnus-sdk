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
		public CavrnusLivePropertyUpdate<T> transientUpdater = null;
		private T lastPostedTransientValue;

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

		//Only really for CoPresence
		public void ForceBeginTransientUpdate()
		{
			transientUpdater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					sync.Context.UniqueContainerName, sync.PropName,
					sync.GetValue());
		}

		const float endChangeTimeGap = .3f;
		float lastChangeTime;

		private void TryUpdate()
		{
			//Don't do anything until we've loaded the space
			if (spaceConn == null) return;

			T currPropertyValue;

			if(transientUpdater == null)
			{
				if (typeof(T) == typeof(Color))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetColorPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else if (typeof(T) == typeof(string))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetStringPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else if (typeof(T) == typeof(bool))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetBoolPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else if (typeof(T) == typeof(Vector4))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetVectorPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else if (typeof(T) == typeof(float))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetFloatPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else if (typeof(T) == typeof(CavrnusTransformData))
				{
					currPropertyValue = (T)(object)CavrnusFunctionLibrary.GetTransformPropertyValue(spaceConn,
						sync.Context.UniqueContainerName, sync.PropName);
				}
				else
				{
					throw new Exception($"Property value of type {typeof(T)} is not supported by CavrnusPostSynchronizedProperty yet!");
				}
			}
			else
			{
				currPropertyValue = lastPostedTransientValue;
			}

			T currUnityVal = sync.GetValue();
			bool hasChange = !currPropertyValue.Equals(currUnityVal);

			//Transforms can sometimes be a little wobbly w/ gravity etc.  Wanna give them a LITTLE bit of epsilon
			if (typeof(T) == typeof(CavrnusTransformData))
			{
				if(currPropertyValue is CavrnusTransformData td && currUnityVal is CavrnusTransformData utd)
				{
					hasChange = !td.Position.AlmostEquals(utd.Position, .001f) || !td.EulerAngles.AlmostEquals(utd.EulerAngles, .1f) || !td.Scale.AlmostEquals(utd.Scale, .001f);
				}
			}

			if (!hasChange) 
			{
				bool isUserProperty = sync.Context.UniqueContainerName.StartsWith("users/");
                //This change has timed out, time to finalize it
                if (!isUserProperty && transientUpdater != null && Time.time - lastChangeTime > endChangeTimeGap) 
				{
					lastPostedTransientValue = currUnityVal;
					transientUpdater.UpdateWithNewData(lastPostedTransientValue);
					transientUpdater.Finish();
					transientUpdater = null;
				}

				//Our transform is synchronized w/ the server, so don't do any posting
				return;
			}

			lastChangeTime = Time.time;

			if (transientUpdater == null)
			{
				lastPostedTransientValue = currUnityVal;
				transientUpdater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					sync.Context.UniqueContainerName, sync.PropName,
					lastPostedTransientValue);
			}
			else
			{
				lastPostedTransientValue = currUnityVal;
				transientUpdater.UpdateWithNewData(lastPostedTransientValue); 
			}
		}

		public void Dispose() { disp.Dispose(); }
	}
}