using CavrnusSdk.UI;
using UnityEditor;
using UnityEngine;

namespace CavrnusSdk.Editor.UI
{
    [CustomEditor(typeof(CavrnusRotationAnimator))]
    public class RotationAnimatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = (CavrnusRotationAnimator) target;

            if (GUILayout.Button("SetBegin")) {
                script.SetBegin();
            }
            if (GUILayout.Button("SetEnd")) {
                script.SetEnd();
            }
        }
    }
}