using System;
using Collab.Base.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.StreamBoards
{
    public class CavrnusStreamBoard : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        private IDisposable profileDisposable;

        public void UpdateAndBindUserTexture(CavrnusUser user)
        {
            if (user == null) 
                ResetStream();

            profileDisposable = user?.VideoTexture.Bind(SetImageAndAspectRatio);
        }

        public void UpdateTexture(Sprite sp)
        {
            SetImageAndAspectRatio(sp);
        }

        private void SetImageAndAspectRatio(Sprite sp)
        {
            if (sp != null) {
                image.sprite = sp;
                aspectRatioFitter.aspectRatio = (float) sp.texture.width / (float) sp.texture.height;
            }
            else
                ResetStream();
        }

        private void ResetStream()
        {
            image.sprite = null;
            profileDisposable?.Dispose();
        }
                
        private void OnDestroy() => profileDisposable?.Dispose();
    }
}