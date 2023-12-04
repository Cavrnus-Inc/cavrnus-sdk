using System;
using Collab.Base.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.XR.Widgets
{
    public class WidgetUserProfileImage : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        private IDisposable profileDisposable;

        public void Setup(CavrnusUser cu)
        {
            profileDisposable = cu.ProfileAndVideoTexture.Bind(vidTex =>
            {
                image.sprite = vidTex;
                if(vidTex != null)
                    aspectRatioFitter.aspectRatio = (float)vidTex.texture.width / (float)vidTex.texture.height;
            });
        }

        private void OnDestroy()
        {
            profileDisposable?.Dispose();
        }
    }
}