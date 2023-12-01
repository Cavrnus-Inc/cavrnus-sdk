using System;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk
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

			CavrnusSpaceJoinEvent.OnAnySpaceConnection(sc => spaceConn = sc);

			disp = CavrnusHelpers.Scheduler.ExecInMainThreadRepeating(pollingTime, TryUpdate);
		}

		const float endChangeTimeGap = .3f;
		float lastChangeTime;

		private void TryUpdate()
		{
			//Don't do anything until we've loaded the space
			if (spaceConn == null) return;

			T currPropertyValue;
			if (typeof(T) == typeof(Color)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetColorPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(string)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetStringPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(bool)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetBooleanPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(Vector4)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetVectorPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(float)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetFloatPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else if (typeof(T) == typeof(CavrnusPropertyHelpers.TransformData)) {
				currPropertyValue = (T) (object) CavrnusPropertyHelpers.GetTransformPropertyValue(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName);
			}
			else {
				throw new Exception(
					$"Property value of type {typeof(T)} is not supported by CavrnusPostSynchronizedProperty yet!");
			}

			T currUnityVal = sync.GetValue();
			bool hasChange = !currPropertyValue.Equals(currUnityVal);

			if (!hasChange) {
				//This change has timed out, time to finalize it
				if (updater != null && Time.time - lastChangeTime > endChangeTimeGap) {
					updater.UpdateWithNewData(sync.GetValue());
					updater.Finish();
					updater = null;
				}

				//Our transform is synchronized w/ the server, so don't do any posting
				return;
			}

			lastChangeTime = Time.time;

			if (updater == null) {
				updater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					sync.Context.UniqueContainerPath, sync.PropName,
					sync.GetValue());
			}
			else { updater.UpdateWithNewData(sync.GetValue()); }
		}

		public void Dispose() { disp.Dispose(); }
	}
}