using System;
using System.Collections;
using CavrnusCore;
using CavrnusSdk.Setup;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CavrnusSdk.MultiplayerGame
{
    public static class CavrnusSampleHelpers
    {
        public static void DestroyAllChildren(this Transform t)
        {
            if (t.childCount == 0) return;
            
            for (var i = t.childCount - 1; i >= 0; i--)
                Object.Destroy(t.GetChild(i).gameObject);
        }
        
        public static Vector3 ToIso(this Vector3 input)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            return matrix.MultiplyPoint3x4(input);
        }

        public static void GetCavrnusUserFlagInParent(this GameObject go, Action<CavrnusUserFlag> onFoundUserFlag)
        {
            CavrnusStatics.Scheduler.ExecCoRoutine(GetCavrnusUserFlagRoutine(go, onFoundUserFlag));
        }
        
        public static void GetCavrnusUserFlagInParent(this Transform t, Action<CavrnusUserFlag> onFoundUserFlag)
        {
            CavrnusStatics.Scheduler.ExecCoRoutine(GetCavrnusUserFlagRoutine(t.gameObject, onFoundUserFlag));
        }

        private static IEnumerator GetCavrnusUserFlagRoutine(GameObject go, Action<CavrnusUserFlag> onFoundUserFlag)
        {
            yield return null;

            var flag = go.GetComponentInParent<CavrnusUserFlag>();
            onFoundUserFlag?.Invoke(flag);
        }
    }
}