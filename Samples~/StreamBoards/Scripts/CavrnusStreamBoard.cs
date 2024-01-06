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

        [SerializeField] private CavrnusStreamContextMenu ctxMenuPrefab;

        private CavrnusSpaceConnection spaceConn;
        private IDisposable profileDisposable;

        private void Start()
        {
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(csc => {
                spaceConn = csc;
            });
        }

        public void OnPointerClick()
        {
            if (spaceConn == null) 
                return;
            
            var ctx = Instantiate(ctxMenuPrefab, null);
            
            ctx.GetComponentInChildren<CavrnusStreamContextMenu>().Setup(spaceConn.UsersList.Users, user => {
                if (user == null) {
                    image.sprite = null;
                    profileDisposable?.Dispose();
                }
                
                profileDisposable = user?.VideoTexture.Bind(vidTex =>
                {
                    if (vidTex != null) {
                        image.sprite = vidTex;
                        aspectRatioFitter.aspectRatio = (float)vidTex.texture.width / (float)vidTex.texture.height;
                    }
                    else {
                        image.sprite = null;
                        profileDisposable?.Dispose();
                    }
                });
                
                Destroy(ctx.gameObject);
            });
        }
                
        private void OnDestroy()
        {
            profileDisposable?.Dispose();
        }
    }
}