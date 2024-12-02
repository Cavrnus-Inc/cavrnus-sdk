using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collab.Base.Math;
using UnityEngine;

namespace UnityBase
{
    public static class TransformMatrixConversion
    {
        public static Matrix34 UnityLocalToMatrix(Transform t) => UnityToMatrix(t.localPosition, t.localRotation, t.localScale);
		public static Matrix34 UnityWorldToMatrix(Transform t) => UnityToMatrix(t.localToWorldMatrix);

		public static Matrix34 UnityToMatrix(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            Matrix4x4 m = Matrix4x4.TRS(pos, rot, scale);
            Matrix34 r = UnityToMatrix(m);
            return r;
        }

        public static Matrix34 UnityToMatrix(Matrix4x4 m)
        {
            return new Matrix34(m.GetColumn(0).ToFloat3(), m.GetColumn(1).ToFloat3(), m.GetColumn(2).ToFloat3(), m.GetColumn(3).ToFloat3());
        }

        public static Matrix4x4 MatrixToUnity(Matrix34 m)
        {
            return new Matrix4x4(m.A.ToVec4(0f), m.B.ToVec4(0f), m.C.ToVec4(0f), m.D.ToVec4(1f));
        }
        public static void MatrixToUnity(Matrix34 m, out Matrix4x4 setTo)
        {
            setTo = new Matrix4x4(m.A.ToVec4(0f), m.B.ToVec4(0f), m.C.ToVec4(0f), m.D.ToVec4(1f));
        }

        public static void MatrixToUnity(Matrix34 m, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            Matrix4x4 um;
            MatrixToUnity(m, out um);

            pos = um.GetColumn(3);

            rot = Quaternion.LookRotation(um.GetColumn(2), um.GetColumn(1));
            Matrix4x4 rotationOnly = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);

            scale = new Vector3(Vector3.Dot(um.GetColumn(0), rotationOnly.GetColumn(0))
            				, Vector3.Dot(um.GetColumn(1), rotationOnly.GetColumn(1))
            				, Vector3.Dot(um.GetColumn(2), rotationOnly.GetColumn(2)));
        }

        public static void MatrixToUnityLocal(Matrix34 m, Transform setTo)
        {
            Vector3 pos, scale;
            Quaternion rot;
            MatrixToUnity(m, out pos, out rot, out scale);

            setTo.localPosition = pos;
            setTo.localRotation = rot;
            setTo.localScale = scale;
        }

        public static bool Matches(Matrix4x4 a, Matrix4x4 b)
        {
            return Mathf.Approximately(a.m00, b.m00)
                   && Mathf.Approximately(a.m01, b.m01)
                   && Mathf.Approximately(a.m02, b.m02)
                   && Mathf.Approximately(a.m03, b.m03)
                   && Mathf.Approximately(a.m10, b.m10)
                   && Mathf.Approximately(a.m11, b.m11)
                   && Mathf.Approximately(a.m12, b.m12)
                   && Mathf.Approximately(a.m13, b.m13)
                   && Mathf.Approximately(a.m20, b.m20)
                   && Mathf.Approximately(a.m21, b.m21)
                   && Mathf.Approximately(a.m22, b.m22)
                   && Mathf.Approximately(a.m23, b.m23)
                   && Mathf.Approximately(a.m30, b.m30)
                   && Mathf.Approximately(a.m31, b.m31)
                   && Mathf.Approximately(a.m32, b.m32)
                   && Mathf.Approximately(a.m33, b.m33)
                ;
        }
    }
}
