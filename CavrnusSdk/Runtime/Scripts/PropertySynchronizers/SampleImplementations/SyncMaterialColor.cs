using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncMaterialColor : CavrnusValueSyncColor
    {
        [SerializeField] private string colorPropertyName = "_BaseColor";

        public Material TargetMaterial;
        
        public override Color GetValue()
        {
            return TargetMaterial.GetColor(colorPropertyName);
        }
        
        public override void SetValue(Color value) { 
            TargetMaterial.SetColor(colorPropertyName, value);
        }
    }
}