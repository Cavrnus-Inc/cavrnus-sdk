using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Math;
using Collab.Proxy.Comm.LiveTypes;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.TransformProp;
using UnityEngine;
using UnityEngine.Assertions;
using Float3 = Collab.Base.Math.Float3;
using Float4 = Collab.Base.Math.Float4;
using TransformUpdateLookAt = Collab.Proxy.Prop.TransformProp.TransformUpdateLookAt;
using TransformUpdateScaleNonuniform = Collab.Proxy.Prop.TransformProp.TransformUpdateScaleNonuniform;
using TransformUpdateScaleUniform = Collab.Proxy.Prop.TransformProp.TransformUpdateScaleUniform;

namespace UnityBase.Objects
{
	public static class UnityTransformResolver
	{
		public static void ResolveIntoUnityTransform(this TransformComplete trans, Transform ut)
		{
			// TODO <TRANSFORMTYPESWITCH>
			switch (trans.startData)
			{
				case TransformDataSRT srt:
					ut.localPosition = srt.translation.ToVec3();
					ut.localEulerAngles = srt.euler.ToVec3();
					ut.localScale = srt.scale.ToVec3();
					break;
				case TransformDataLookAt la:
					ut.localPosition = la.eye.ToVec3();
					ut.LookAt(la.lookAt.ToVec3(), la.nominalUp.ToVec3());
					break;
				case TransformDataSQT sqt:
					ut.localPosition = sqt.translation.ToVec3();
					ut.localRotation = sqt.quat.ToQuat();
					ut.localScale = sqt.scale.ToVec3();
					break;
				case null:
					break;
				default:
					throw new NotImplementedException($"Unexpected TransformData of type '{trans.startData.GetType().Name}'");
			}

			foreach (var tu in (trans.updates).EmptyIfNull())
			{
				switch (tu)
				{
					case TransformUpdateShift s:
						ut.localPosition += s.moveBy.ToVec3();
						break;
					case TransformUpdateLookAt la:
					{
						var fromToRot = Quaternion.FromToRotation(ut.forward, la.rotateToFacePt.ToVec3() - ut.position);
						fromToRot.ToAngleAxis(out var ang, out var axis);
						ut.Rotate(axis, (float) (ang * la.percentageToMove), Space.World);
						break;
					}
					case TransformUpdateScaleUniform su:
						ut.localScale *= (float)su.scale;
						break;
					case TransformUpdateScaleNonuniform snu:
						ut.localScale = new Vector3(ut.localScale.x * snu.scale.X, ut.localScale.y * snu.scale.Y, ut.localScale.z * snu.scale.Z);
						break;
					case TransformUpdateRotateEuler re:
						ut.Rotate(re.euler.ToVec3());
						break;
					case TransformUpdateRotateQuat rq:
					{
						rq.quat.ToQuat().ToAngleAxis(out var ang, out var axis);
						ut.Rotate(axis, ang, Space.Self);
						break;
					}
					case TransformUpdateToEuler rte:
						// Find current world euler, rotate partially based on each channel.
						var curEuler = ut.eulerAngles;
						ut.eulerAngles = Vector3.LerpUnclamped(curEuler, rte.rotateToEuler.ToVec3(), (float)rte.percentageToMove);
						break;
					case TransformUpdateToQuat rtq:
						ut.rotation = Quaternion.LerpUnclamped(ut.rotation, rtq.rotateToQuat.ToQuat(), (float) rtq.percentageToMove);
						break;
					default:
						throw new NotImplementedException($"Unexpected TransformUpdate type: {tu.GetType().Name}");
				}
			}
		}

		public static Quaternion ResolveOrientation(this TransformComplete trans)
		{
			Quaternion r;
			Vector3 pos;
			switch (trans.startData)
			{
				case TransformDataSRT srt:
					pos = srt.translation.ToVec3();
					r = Quaternion.Euler(srt.euler.ToVec3());
					break;
				case TransformDataLookAt la: // TODO: World-space lookat position?
					pos = la.eye.ToVec3();
					r = Quaternion.LookRotation(Float3.FromTo(la.eye, la.lookAt).ToVec3(), la.nominalUp.ToVec3());
					break;
				case TransformDataSQT sqt:
					pos = sqt.translation.ToVec3();
					r = sqt.quat.ToQuat();
					break;
				default:
					throw new NotImplementedException($"Unexpected TransformData of type '{trans.startData.GetType().Name}'");
			}

			foreach (var tu in trans.updates.EmptyIfNull())
			{
				switch (tu)
				{
					case TransformUpdateShift s:
						break;
					case TransformUpdateLookAt la:
					{
						// TODO We need the position here to do this right.
						var look = Quaternion.LookRotation(la.rotateToFacePt.ToVec3() - pos);
						r = Quaternion.Slerp(r, look, (float)la.percentageToMove);
						break;
					}
					case TransformUpdateScaleUniform su:
						break;
					case TransformUpdateScaleNonuniform snu:
						break;
					case TransformUpdateRotateEuler re:
						r *= Quaternion.Euler(re.euler.ToVec3());
						break;
					case TransformUpdateRotateQuat rq:
						r *= rq.quat.ToQuat();
						break;
					case TransformUpdateToEuler rte:
					{
						Quaternion targetRot = Quaternion.Euler(rte.rotateToEuler.X, rte.rotateToEuler.Y, rte.rotateToEuler.Z);
						r = Quaternion.Slerp(r, targetRot, (float)rte.percentageToMove);
						break;
					}
					case TransformUpdateToQuat rtq:
					{
						Quaternion targetRot = rtq.rotateToQuat.ToQuat();
						r = Quaternion.Slerp(r, targetRot, (float)rtq.percentageToMove);
						break;
					}
					default:
						throw new NotImplementedException($"Unexpected TransformUpdate type: {tu.GetType().Name}");
				}
			}

			return r;
		}

		public static Matrix4x4 ResolveIntoUnityMatrix(this TransformComplete trans, Matrix4x4? parentPosition)
		{
			// TODO <TRANSFORMTYPESWITCH>
			Matrix4x4 r = parentPosition ?? Matrix4x4.identity;
			Matrix4x4 parentInverse = parentPosition?.inverse ?? Matrix4x4.identity;
			switch (trans.startData)
			{
				case TransformDataSRT srt:
					r *= Matrix4x4.TRS(srt.translation.ToVec3(), Quaternion.Euler(srt.euler.ToVec3()), srt.scale.ToVec3() );
					break;
				case TransformDataLookAt la: // TODO: World-space lookat position?
					r *= Matrix4x4.LookAt(la.eye.ToVec3(), parentInverse * la.lookAt.ToVec3(), la.nominalUp.ToVec3());
					break;
				case TransformDataSQT sqt:
					r *= Matrix4x4.TRS(sqt.translation.ToVec3(), sqt.quat.ToQuat(), sqt.scale.ToVec3());
					break;
				default:
					throw new NotImplementedException($"Unexpected TransformData of type '{trans.startData.GetType().Name}'");
			}

			foreach (var tu in trans.updates.EmptyIfNull())
			{
				switch (tu)
				{
					case TransformUpdateShift s:
						r.SetColumn(3, r.GetColumn(3)+s.moveBy.ToVec4(0f));
						break;
					case TransformUpdateLookAt la:
						{
							Float3 fromTo = Float3.FromTo(r.GetPosition().ToFloat3(), la.rotateToFacePt).Normalize();

							var toRotFull = Quaternion.FromToRotation(r.GetPosition(), fromTo.ToVec3());
							var toRotPartial = Quaternion.Slerp(Quaternion.identity, toRotFull, (float)la.percentageToMove);
							r *= Matrix4x4.Rotate(toRotPartial);
							break;
						}
					case TransformUpdateScaleUniform su:
						r *= Matrix4x4.Scale(new Vector3((float)su.scale, (float)su.scale, (float)su.scale));
						break;
					case TransformUpdateScaleNonuniform snu:
						r *= Matrix4x4.Scale(snu.scale.ToVec3());
						break;
					case TransformUpdateRotateEuler re:
						r *= Matrix4x4.Rotate(Quaternion.Euler(re.euler.X, re.euler.Y, re.euler.Z));
						break;
					case TransformUpdateRotateQuat rq:
						r *= Matrix4x4.Rotate(rq.quat.ToQuat());
						break;
					case TransformUpdateToEuler rte:
						{
							Quaternion targetRot = Quaternion.Euler(rte.rotateToEuler.X, rte.rotateToEuler.Y, rte.rotateToEuler.Z);
							// Uuuuugh Why did I do this to myself...
							Matrix4x4 toEuMtx = Matrix4x4.Rotate(targetRot);
							// Compute rotation of forward-vec, then correct the up-vec?
							Quaternion forwardRot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(r.GetForward(), toEuMtx.GetForward()), (float)rte.percentageToMove);
							r *= Matrix4x4.Rotate(forwardRot);
							Quaternion upRot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(r.GetUp(), toEuMtx.GetUp()), (float)rte.percentageToMove);
							r *= Matrix4x4.Rotate(upRot);
							break;
						}
					case TransformUpdateToQuat rtq:
						{
							Quaternion targetRot = rtq.rotateToQuat.ToQuat();
							// Uuuuugh Why did I do this to myself...
							Matrix4x4 toEuMtx = Matrix4x4.Rotate(targetRot);
							// Compute rotation of forward-vec, then correct the up-vec?
							Quaternion forwardRot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(r.GetForward(), toEuMtx.GetForward()), (float)rtq.percentageToMove);
							r *= Matrix4x4.Rotate(forwardRot);
							Quaternion upRot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(r.GetUp(), toEuMtx.GetUp()), (float)rtq.percentageToMove);
							r *= Matrix4x4.Rotate(upRot);
							break;
						}
					default:
						throw new NotImplementedException($"Unexpected TransformUpdate type: {tu.GetType().Name}");
				}
			}

			return r;
		}
	}

	public static class UnityTransformSubPropertyRegistration
	{
		public static IDisposable RegisterLiveTransformSubProperties(IProperty<TransformComplete> p, Transform t)
		{
			// TODO: These aren't dependent on the actual tc value, which is certainly more efficient but breaks many expectations... Not sure if we should keep it like this or move transform logic further into properties.
			p.RegisterSubProperty("worldUp", new DelegatedTransformVectorSubProperty((tc) => t.up.ToFloat4(1f)));
			p.RegisterSubProperty("worldUp.x", new DelegatedTransformScalarSubProperty((tc) => t.up.x));
			p.RegisterSubProperty("worldUp.y", new DelegatedTransformScalarSubProperty((tc) => t.up.y));
			p.RegisterSubProperty("worldUp.z", new DelegatedTransformScalarSubProperty((tc) => t.up.z));
			p.RegisterSubProperty("worldForward", new DelegatedTransformVectorSubProperty((tc) => t.forward.ToFloat4(1f)));
			p.RegisterSubProperty("worldForward.x", new DelegatedTransformScalarSubProperty((tc) => t.forward.x));
			p.RegisterSubProperty("worldForward.y", new DelegatedTransformScalarSubProperty((tc) => t.forward.y));
			p.RegisterSubProperty("worldForward.z", new DelegatedTransformScalarSubProperty((tc) => t.forward.z));
			p.RegisterSubProperty("worldRight", new DelegatedTransformVectorSubProperty((tc) => t.right.ToFloat4(1f)));
			p.RegisterSubProperty("worldRight.x", new DelegatedTransformScalarSubProperty((tc) => t.right.x));
			p.RegisterSubProperty("worldRight.y", new DelegatedTransformScalarSubProperty((tc) => t.right.y));
			p.RegisterSubProperty("worldRight.z", new DelegatedTransformScalarSubProperty((tc) => t.right.z));
			p.RegisterSubProperty("worldEuler", new DelegatedTransformVectorSubProperty((tc) => t.eulerAngles.ToFloat4(1f)));
			p.RegisterSubProperty("worldEuler.x", new DelegatedTransformScalarSubProperty((tc) => t.eulerAngles.x));
			p.RegisterSubProperty("worldEuler.y", new DelegatedTransformScalarSubProperty((tc) => t.eulerAngles.y));
			p.RegisterSubProperty("worldEuler.z", new DelegatedTransformScalarSubProperty((tc) => t.eulerAngles.z));
			p.RegisterSubProperty("worldQuat", new DelegatedTransformVectorSubProperty((tc) => t.rotation.ToFloat4()));
			p.RegisterSubProperty("worldScale", new DelegatedTransformVectorSubProperty((tc) => t.lossyScale.ToFloat4(1f)));
			p.RegisterSubProperty("worldPos", new DelegatedTransformVectorSubProperty((tc)=>t.position.ToFloat4(1f)));
			p.RegisterSubProperty("worldPos.x", new DelegatedTransformScalarSubProperty((tc) => t.position.x));
			p.RegisterSubProperty("worldPos.y", new DelegatedTransformScalarSubProperty((tc) => t.position.y));
			p.RegisterSubProperty("worldPos.z", new DelegatedTransformScalarSubProperty((tc) => t.position.z));

			return new DelegatedDisposalHelper(() =>
			{
				p.UnregisterSubProperty("worldUp");
				p.UnregisterSubProperty("worldUp.x");
				p.UnregisterSubProperty("worldUp.y");
				p.UnregisterSubProperty("worldUp.z");
				p.UnregisterSubProperty("worldForward");
				p.UnregisterSubProperty("worldForward.x");
				p.UnregisterSubProperty("worldForward.y");
				p.UnregisterSubProperty("worldForward.z");
				p.UnregisterSubProperty("worldRight");
				p.UnregisterSubProperty("worldRight.x");
				p.UnregisterSubProperty("worldRight.y");
				p.UnregisterSubProperty("worldRight.z");
				p.UnregisterSubProperty("worldEuler");
				p.UnregisterSubProperty("worldEuler.x");
				p.UnregisterSubProperty("worldEuler.y");
				p.UnregisterSubProperty("worldEuler.z");
				p.UnregisterSubProperty("worldQuat");
				p.UnregisterSubProperty("worldScale");
				p.UnregisterSubProperty("worldPos");
				p.UnregisterSubProperty("worldPos.x");
				p.UnregisterSubProperty("worldPos.y");
				p.UnregisterSubProperty("worldPos.z");
			});
		}
	}

	public class DelegatedTransformVectorSubProperty : ASubProperty<Float4, TransformComplete>
	{
		private Func<TransformComplete, Float4> delegated;

		public DelegatedTransformVectorSubProperty(Func<TransformComplete, Float4> delegated)
		{
			this.delegated = delegated;
		}

		protected override Float4 Compute(TransformComplete src, IComputationCache cc, IGeneratorContext ctx)
		{
			return delegated(src);
		}
	}

	public class DelegatedTransformScalarSubProperty : ASubProperty<double, TransformComplete>
	{
		private Func<TransformComplete, double> delegated;

		public DelegatedTransformScalarSubProperty(Func<TransformComplete, double> delegated)
		{
			this.delegated = delegated;
		}

		protected override double Compute(TransformComplete src, IComputationCache cc, IGeneratorContext ctx)
		{
			return delegated(src);
		}
	}

	public static class UnityMatrixExtensions
	{
		public static Vector3 GetPosition(this Matrix4x4 m)
		{
			return new Vector3(m.m03, m.m13, m.m23);
		}

		public static void SetPosition(this Matrix4x4 m, Vector3 pos)
		{
			m.m03 = pos.x;
			m.m13 = pos.y;
			m.m23 = pos.z;
		}

		public static Vector3 GetForward(this Matrix4x4 m)
		{
			return new Vector3(m.m02, m.m12, m.m22);
		}
		public static Vector3 GetUp(this Matrix4x4 m)
		{
			return new Vector3(m.m01, m.m11, m.m21);
		}
	}
}
