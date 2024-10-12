using System;
using System.Collections;
using CavrnusCore;
using CavrnusSdk.API;
using UnityBase;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.UI
{
    public static class CavrnusUIHelpers
    {
        public static IDisposable BindUserPicToImage(CavrnusUser user, Image image, AspectRatioFitter ratioFitter)
        {
            var picDisp = user.BindProfilePic(profilePic =>
            {
                if (image != null) {
                    image.sprite = profilePic;
                }
                if (profilePic != null) {
                    ratioFitter.aspectRatio = (float)profilePic.texture.width / (float)profilePic.texture.height;
                }
            });

            return picDisp;
        }
        
        
        public static IDisposable BindUserStreamToRawImage(CavrnusUser user, RawImage image, AspectRatioFitter ratioFitter)
        {
            var videoDisp = user.BindUserVideoFrames(tex => {
                CavrnusStatics.Scheduler.ExecCoRoutine(AssignVidTexture(tex, image, ratioFitter));
            });

            return videoDisp;
        }
        
        private static IEnumerator AssignVidTexture(TextureWithUVs tex, RawImage image, AspectRatioFitter ratioFitter)
        {
            if (tex.Texture.width > 0 && tex.Texture.height > 0)
                ratioFitter.aspectRatio = (float) tex.Texture.width / (float) tex.Texture.height;
            else
                ratioFitter.aspectRatio = 1.67f;
			
            yield return new WaitForSeconds(1f); // Need delay to handle if user is already streaming when loading space

            image.texture = tex.Texture;
            image.uvRect = tex.UVRect;
        }
    }
}