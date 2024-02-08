using System;
using System.Linq;
using Collab.Proxy.Comm.LiveTypes;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.JournalInterop;
using Collab.Proxy.Prop.TransformProp;
using UnityBase;
using UnityEngine;
using CavrnusSdk.API;

namespace CavrnusCore
{
	internal class CavrnusPropertyHelpers : MonoBehaviour
	{
		private static void ResolveContainerPath(ref string propertyId, ref string containerId)
		{
			//Ignore the path completely and resolve from root
			if (propertyId.StartsWith("/")) {
				var split = propertyId.Split('/', StringSplitOptions.RemoveEmptyEntries);
				propertyId = split.Last();
				containerId = String.Join("/", split.Take(split.Length - 1));
			}
			//Add my path to the overall path
			else if (propertyId.Contains("/")) {
				var split = propertyId.Split('/', StringSplitOptions.RemoveEmptyEntries);
				propertyId = split.Last();
				containerId = String.Join("/", split.Take(split.Length - 1)) + "/" + containerId;
            }
		}

		private static void CheckCommonErrors(CavrnusSpaceConnection spaceConn, string containerId,
		                                      string propertyId)
		{
			if (spaceConn == null)
				throw new ArgumentException("RoomConnection is null!  Has the space finished loading yet?");
		}

		private static PropertySetManager MyContainer(CavrnusSpaceConnection spaceConn, string containerId)
		{
			var myContainerId = new PropertyId(containerId);
			return spaceConn.RoomSystem.PropertiesRoot.SearchForContainer(myContainerId);
		}

		internal static CavrnusLivePropertyUpdate<T> BeginContinuousPropertyUpdate<T>(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, T val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<T>(spaceConn, containerId, propertyId, val);
		}

		#region Color Props

		internal static bool ColorPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                      string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForColorProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineColorPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Color defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.DefineColorProperty(propertyId, defaultVal.ToColor4F(),
			                                       new Collab.Proxy.Prop.ColorProp.ColorPropertyMetadata() {
				                                       Name = propertyId
												   });
		}

		internal static Color GetColorPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                          string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetColorProperty(propertyId).Current.Value.Value.ToColor();
		}

		internal static IDisposable BindToColorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Action<Color> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetColorProperty(propertyId).Bind(c => onValueChanged(c.ToColor()));
		}

		internal static CavrnusLivePropertyUpdate<Color> LiveUpdateColorProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Color val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<Color>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateColorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                           string propertyId, Color value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForColorProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildColorPropertyOp(myProp.AbsoluteId,
			                                                       new ColorPropertyValue() {
				                                                       Constant = value.ToColor4().ToPb()
			                                                       });

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

		#region String Props

		internal static bool StringPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                       string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForStringProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineStringPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, string defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.DefineStringProperty(propertyId, defaultVal,
			                                        new Collab.Proxy.Prop.StringProp.StringPropertyMetadata() {
				                                        Name = propertyId
													});
		}

		internal static string GetStringPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetStringProperty(propertyId).Current.Value.Value;
		}

		internal static IDisposable BindToStringProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Action<string> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetStringProperty(propertyId).Bind(c => { onValueChanged(c); });
		}

		internal static CavrnusLivePropertyUpdate<string> LiveUpdateStringProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, string val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<string>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateStringProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId, string value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForStringProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildStringPropertyOp(myProp.AbsoluteId,
			                                                        new StringPropertyValue() {Constant = value});

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

		#region Boolean Props

		internal static bool BooleanPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                     string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForBooleanProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineBooleanPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                             string propertyId, bool defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.DefineBooleanProperty(propertyId, defaultVal,
			                                         new Collab.Proxy.Prop.BooleanProp.BooleanPropertyMetadata() {
				                                         Name = propertyId
													 });
		}

		internal static bool GetBooleanPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                        string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetBooleanProperty(propertyId).Current.Value.Value;
		}

		internal static IDisposable BindToBooleanProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                             string propertyId, Action<bool> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetBooleanProperty(propertyId).Bind(c => onValueChanged(c));
		}

		internal static CavrnusLivePropertyUpdate<bool> LiveUpdateBooleanProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, bool val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<bool>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateBooleanProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                          string propertyId, bool value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForBooleanProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildBoolPropertyOp(myProp.AbsoluteId,
			                                                      new BooleanPropertyValue() {Constant = value});

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

		#region Float Props

		internal static bool FloatPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                      string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForScalarProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineFloatPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, float defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.DefineScalarProperty(propertyId, defaultVal,
			                                        new Collab.Proxy.Prop.ScalarProp.ScalarPropertyMetadata() {
				                                        Name = propertyId
													});
		}

		internal static float GetFloatPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                          string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return (float) myContainer.GetScalarProperty(propertyId).Current.Value.Value;
		}

		internal static IDisposable BindToFloatProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Action<float> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetScalarProperty(propertyId).Bind(c => onValueChanged((float) c));
		}

		internal static CavrnusLivePropertyUpdate<float> LiveUpdateFloatProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, float val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<float>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateFloatProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                           string propertyId, float value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForScalarProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildScalarPropertyOp(myProp.AbsoluteId,
			                                                        new ScalarPropertyValue() {Constant = value});

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

		#region Vector Props

		internal static bool VectorPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                       string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForVectorProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineVectorPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Vector4 defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.DefineVectorProperty(propertyId, defaultVal.ToFloat4(),
			                                        new Collab.Proxy.Prop.VectorProp.VectorPropertyMetadata() {
				                                        Name = propertyId
													});
		}

		internal static Vector4 GetVectorPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                             string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetVectorProperty(propertyId).Current.Value.Value.ToVec4();
		}

		internal static IDisposable BindToVectorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Action<Vector4> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetVectorProperty(propertyId).Bind(c => onValueChanged(c.ToVec4()));
		}

		internal static CavrnusLivePropertyUpdate<Vector4> LiveUpdateVectorProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Vector4 val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<Vector4>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateVectorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId, Vector4 value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForVectorProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildVectorPropertyOp(myProp.AbsoluteId,
			                                                        new VectorPropertyValue() {
				                                                        Constant = value.ToFloat4().ToPb()
			                                                        });

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

		#region Transform Props

		internal static bool TransformPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                          string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForTransformProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineTransformPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                                  string propertyId, Vector3 defaultPos,
		                                                  Vector3 defaultRot, Vector3 defaultScl)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			var defaultTrans = new TransformComplete() {
				startData = new TransformDataSRT() {
					translation = defaultPos.ToFloat3(), euler = defaultRot.ToFloat3(), scale = defaultScl.ToFloat3(),
				}
			};

			return myContainer.DefineTransformProperty(propertyId, defaultTrans,
			                                           new Collab.Proxy.Prop.TransformProp.TransformPropertyMetadata() {
				                                           Name = propertyId
													   });
		}

		internal static CavrnusTransformData GetTransformPropertyValue(CavrnusSpaceConnection spaceConn,
		                                                      string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			var val = myContainer.GetTransformProperty(propertyId).Current.Value.Value;

			CavrnusTransformData res = new CavrnusTransformData(val.ResolveTranslation().ToVec3(), val.ResolveEuler().ToVec3(),
			                                      val.ResolveScaleVector().ToVec3());
			return res;
		}

		internal static IDisposable BindToTransformProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                                  string propertyId,
		                                                  Action<CavrnusTransformData> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetTransformProperty(propertyId).Bind(t => {
				onValueChanged(new CavrnusTransformData(t.ResolveTranslation().ToVec3(), t.ResolveEuler().ToVec3(),
				               t.ResolveScaleVector().ToVec3()));
			});
		}

		

		internal static CavrnusLivePropertyUpdate<CavrnusTransformData> LiveUpdateTransformProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Vector3 localPos, Vector3 localRot, Vector3 localScl)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<CavrnusTransformData>(spaceConn, containerId, propertyId,
			                                                          new CavrnusTransformData(localPos, localRot, localScl));
		}

		internal static void UpdateTransformProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Vector3 localPos, Vector3 localRot,
		                                               Vector3 localScl)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForTransformProperty(new PropertyId(propertyId));

			TransformSet trns = new TransformSet() {
				Srt = new TransformSetSRT() {
					TransformPos = new VectorPropertyValue() {Constant = localPos.ToFloat4().ToPb()},
					RotationEuler = new VectorPropertyValue() {Constant = localRot.ToFloat4().ToPb()},
					Scale = new VectorPropertyValue() {Constant = localScl.ToFloat4().ToPb()},
				}
			};

			var op = PropertyOperationHelpers.BuildTransformPropertyOp(myProp.AbsoluteId, trns);

			spaceConn.RoomSystem.Comm.SendJournalEntry(op.ToOp(), null);
		}

		#endregion

	}
}