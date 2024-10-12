using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class SetMaterialTexture : MonoBehaviour
    {
        [SerializeField] private Renderer rend;

        public void SetMainTexture(Texture2D texture)
        {
            rend.material.mainTexture = texture;
        }
    }
}