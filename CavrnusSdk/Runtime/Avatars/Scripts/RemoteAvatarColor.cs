using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.Avatars
{
    public class RemoteAvatarColor : MonoBehaviour
    {
        [SerializeField] private List<Renderer> meshRenderer;
        
        private void Awake()
        {
            SetAvatarRandomColor();
        }

        private void SetAvatarRandomColor()
        {
            var hue = Random.value;
            foreach (var mr in meshRenderer) {
                foreach (var mat in mr.materials) {
                    mat.color = Color.HSVToRGB(hue, 1.0f, 1.0f);
                }
            }
        }
    }
}