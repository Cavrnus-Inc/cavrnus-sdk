using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncMaterial : CavrnusValueSyncString
    {
        [SerializeField] private Renderer rend;
        
        [Space]
        [SerializeField] private List<Material> materials;
        private readonly Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();

        private void Awake()
        {
            materials.ForEach(m => materialLookup.Add(m.name, m));
        }

        public override string GetValue()
        {
            if (rend.material.name.Contains("(Instance)")) {
                var ret = rend.material.name.Replace("(Instance)", "").TrimEnd();
                return ret;
            }
            
            return rend.material.name;
        }

        public override void SetValue(string value)
        {
            if (value == null) return;
            
            if (materialLookup.TryGetValue(value, out var found))
                rend.material = found;
        }
    }
}