using UnityEngine;

namespace CavrnusSdk.Avatars
{
    public class RemoteAvatarColor : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        
        private void Awake()
        {
            SetAvatarRandomColor();
        }

        private void SetAvatarRandomColor()
        {
            var hue = Random.value;
            foreach (var mat in meshRenderer.materials)
                mat.color = Color.HSVToRGB(hue, 1.0f, 1.0f);
        }
    }
}