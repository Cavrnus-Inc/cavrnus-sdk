using Collab.LiveRoomSystem.GameEngineConnector;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusUnityConnector : IGameEngineConnector
	{
		private PropertySetManager propsRoot;
		private IUnityScheduler scheduler;

		public Dictionary<PropertyId, IManagedEngineObject> ManagedObjectsLookup = new Dictionary<PropertyId, IManagedEngineObject>();

		public CavrnusUnityConnector(IUnityScheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		private ManagedRoom managedRoom;
		public void Setup(PropertySetManager propsRoot)
		{
			this.propsRoot = propsRoot;

			managedRoom = new ManagedRoom();
			managedRoom.EstablishRoomProperties(propsRoot.GetContainer(PropertyDefs.RoomGroup));
		}

		public void MakeTransform(PropertySetManager props, PropertyId parent = null)
		{
			var parentLookup = parent != null && ManagedObjectsLookup.ContainsKey(parent) ? ManagedObjectsLookup[parent] : null;

			var mo = new ManagedTransform(props, parentLookup);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeObject(PropertySetManager props, PropertyId mesh,
			List<PropertyId> materials,
			PropertyId parent = null)
		{
			var parentLookup = parent != null && ManagedObjectsLookup.ContainsKey(parent) ? ManagedObjectsLookup[parent] : null;

			var meshLookup = ManagedObjectsLookup[mesh];

			var materialsLookup = new List<IManagedEngineObject>();
			foreach (var material in materials)
				materialsLookup.Add(ManagedObjectsLookup[material]);

			var mo = new ManagedGameObject(props, meshLookup, materialsLookup, parentLookup);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeMesh(PropertySetManager props, Collab.Base.Graphics.Mesh mesh)
		{
			var mo = new ManagedMesh(props, mesh);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeTexture(PropertySetManager props, CreateTextureRequest texReq)
		{
			var mo = new ManagedTexture(props, texReq, scheduler);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeMaterial(PropertySetManager props)
		{
			var mo = new ManagedMaterial(props);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeLight(PropertySetManager props, PropertyId parent)
		{
			var parentLookup = ManagedObjectsLookup[parent];

			var mo = new ManagedLight(props, parentLookup);
			ManagedObjectsLookup[props.AbsoluteId] = mo;
		}

		public void MakeAnimationClip(PropertySetManager props, PropertyId parent,
			CreateAnimationClipRequest req)
		{
			/*var parentLookup = ManagedObjectsLookup[parent];

			var mo = new ManagedAnimation(props, parentLookup, req, ManagedObjectsLookup);
			ManagedObjectsLookup[props.AbsoluteId] = mo;*/
		}

		public void DisposeOfPropsRoot()
		{
			managedRoom?.Dispose();
		}

		public void DisposeOfId(PropertyId disp)
		{
			if(ManagedObjectsLookup.ContainsKey(disp))
				ManagedObjectsLookup[disp].Dispose();
		}
	}
}