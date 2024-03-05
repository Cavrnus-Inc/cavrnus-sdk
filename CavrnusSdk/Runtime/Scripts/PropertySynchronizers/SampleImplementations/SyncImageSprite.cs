using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.PropertySynchronizers.CommonImplementations
{
    public class SyncImageSprite : CavrnusStringPropertySynchronizer
    {
        [SerializeField] private List<Sprite> sprites;
        public readonly Dictionary<string, Sprite> SpriteLookup = new Dictionary<string, Sprite>();

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            sprites.ForEach(t => SpriteLookup.Add(t.name, t));
        }

        public override string GetValue()
        {
            return image.sprite.name;
        }

        public override void SetValue(string value)
        {
            if (SpriteLookup.TryGetValue(value, out var found))
                image.sprite = found;
        }
    }
}