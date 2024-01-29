using Collab.Proxy.Comm.LocalTypes;
using Collab.Proxy.LiveJournal;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ColorProp;
using Collab.Proxy.Comm.LiveTypes;
using Collab.Proxy.Prop.BooleanProp;
using Collab.Proxy.Prop.ScalarProp;
using Collab.Proxy.Prop.StringProp;
using Collab.Proxy.Prop.VectorProp;
using UnityBase;
using UnityEngine;
using Collab.Proxy.Prop.TransformProp;

namespace CavrnusSdk
{
	public class CavrnusLivePropertyUpdate<T>
	{
		private LiveOpHandler<OpPropertyUpdateLive> handler;
		private ITransformProperty specialTransformProp;

		public CavrnusLivePropertyUpdate(CavrnusSpaceConnection spaceConn, string[] pathToContainer,
		                                       string propertyId, T data)
		{
			var myContainerId = PropertyId.FromRelativeStack(pathToContainer);
			var myContainer = spaceConn.RoomSystem.PropertiesRoot.SearchForContainer(myContainerId);

			OpPropertyUpdateLive op = new OpPropertyUpdateLive();
			op.Property = myContainer.AbsoluteId.Push(propertyId);

			if (data is Color c)
				op.Assignment = new ColorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new ColorGeneratorConst(c.ToColor4F()).ToPb(),
				};
			else if (data is bool b)
				op.Assignment = new BooleanPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new BooleanGeneratorConst(b).ToPb(),
				};
			else if (data is string s)
				op.Assignment = new StringPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new StringGeneratorConst(s).ToPb(),
				};
			else if (data is float f)
				op.Assignment = new ScalarPropertyAssignmentLive()
				{
					AssignmentId = "-",
					OverridingPriority = 0,
					GeneratorPb = new ScalarGeneratorConst(f).ToPb(),
				};
			else if (data is Vector4 v)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(v.ToFloat4()).ToPb(),
				};
			else if (data is Vector3 v3)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(new Vector4(v3.x, v3.y, v3.z).ToFloat4()).ToPb(),
				};
			else if (data is Vector2 v2)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(new Vector4(v2.x, v2.y).ToFloat4()).ToPb(),
				};
			else if (data is CavrnusPropertyHelpers.TransformData t)
			{
				op.Assignment = new TransformPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					SetGeneratorPb = BuildApproachTransform(t),
				};
				specialTransformProp = spaceConn.RoomSystem.PropertiesRoot.SearchForTransformProperty(op.Property);
				specialTransformProp.UpdateValue("moverTmp", 1, new TransformSetGeneratorSrt(
					new VectorGeneratorConst(t.LocalPosition.ToFloat4()),
					new VectorGeneratorConst(t.LocalEulerAngles.ToFloat4()),
					new VectorGeneratorConst(t.LocalScale.ToFloat4())));
			}

			

			handler = spaceConn.RoomSystem.LiveOpsSys.Create(op);
			//Debug.Log("Posting First Transient " + Time.time);
			handler.PostAsTransient();
		}

		public void UpdateWithNewData(T data)
		{
			if (data is Color c)
				handler.OpData.Assignment = new ColorPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new ColorGeneratorConst(c.ToColor4F()).ToPb()
				};
			if (data is bool b)
				handler.OpData.Assignment = new BooleanPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new BooleanGeneratorConst(b).ToPb()
				};
			if (data is string s)
				handler.OpData.Assignment = new StringPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new StringGeneratorConst(s).ToPb()
				};
			if (data is float f)
				handler.OpData.Assignment = new ScalarPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new ScalarGeneratorConst(f).ToPb()
				};
			if (data is Vector4 v)
				handler.OpData.Assignment = new VectorPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new VectorGeneratorConst(v.ToFloat4()).ToPb()
				};
			if (data is CavrnusPropertyHelpers.TransformData t)
			{
				handler.OpData.Assignment = new TransformPropertyAssignmentLive()
				{
					AssignmentId = "-",
					SetGeneratorPb = BuildApproachTransform(t),
				};

				specialTransformProp.UpdateValue("moverTmp", 1, new TransformSetGeneratorSrt(
					new VectorGeneratorConst(t.LocalPosition.ToFloat4()),
					new VectorGeneratorConst(t.LocalEulerAngles.ToFloat4()),
					new VectorGeneratorConst(t.LocalScale.ToFloat4())));
			}
			Debug.Log("Posting Transient " + handler.OpData + ", " + Time.time);

			handler.PostAsTransient();
		}

		private TransformSet BuildApproachTransform(CavrnusPropertyHelpers.TransformData t)
		{
			return new TransformSet()
			{
				Approach = new TransformSetApproach()
				{
					To = new TransformSet()
					{
						Srt = new TransformSetSRT()
						{
							TransformPos = new VectorPropertyValue() { Constant = t.LocalPosition.ToFloat4().ToPb() },
							RotationEuler = new VectorPropertyValue() { Constant = t.LocalEulerAngles.ToFloat4().ToPb() },
							Scale = new VectorPropertyValue() { Constant = t.LocalScale.ToFloat4().ToPb() },
						}
					},
					TimeToHalf = new ScalarPropertyValue() { Constant = .1f },
					T = new ScalarPropertyValue() { Ref = new PropertyIdentifier() { Id = "t" } },
				},
			};
		}

		public void Finish()
		{
			if(specialTransformProp != null)
			{
				specialTransformProp.ClearValue("moverTmp");
			}

			//Debug.Log("Posting Final " + Time.time);
			handler.PostAndComplete();
		}

		public void Cancel() { handler.Cancel(); }
	}
}