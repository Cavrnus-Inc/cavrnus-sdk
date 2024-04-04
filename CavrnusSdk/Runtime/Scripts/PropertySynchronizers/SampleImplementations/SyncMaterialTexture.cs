using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncMaterialTexture : CavrnusValueSyncString
    {
        [Space] [SerializeField] private Material material;

        [Space] [SerializeField] private List<Texture> textures;
        private readonly Dictionary<string, Texture> textureLookup = new Dictionary<string, Texture>();

        private void Awake()
        {
            textures.ForEach(t => textureLookup.Add(t.name, t));
        }

        public override string GetValue()
        {
            return material.mainTexture.name;
        }

        public override void SetValue(string value)
        {
            if (textureLookup.TryGetValue(value, out var found)) material.mainTexture = found;
        }
    }
}