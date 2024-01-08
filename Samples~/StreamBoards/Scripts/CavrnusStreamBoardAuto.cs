using System;
using System.Collections.Generic;
using Collab.Base.Collections;
using UnityEngine;

namespace CavrnusSdk.StreamBoards
{
    [RequireComponent(typeof(CavrnusStreamBoard))]
    public class CavrnusStreamBoardAuto : MonoBehaviour
    {
        private readonly Dictionary<CavrnusUser, IHook> streamHooks = new Dictionary<CavrnusUser, IHook>();

        private CavrnusStreamBoard board;
        private CavrnusSpaceConnection spaceConn;
        private IDisposable listDisp;

        private void Awake()
        {
            board = GetComponent<CavrnusStreamBoard>();
        }
        
        private void Start()
        {
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(csc => {
                spaceConn = csc;
                listDisp = spaceConn.UsersList.Users.BindAll(UserAdded, UserRemoved);
            });
        }

        private void UserAdded(CavrnusUser user)
        {
            // This does not provide any board ownership. User B can and will override User A presentation.
            streamHooks.Add(user, user.VideoTexture.Bind(vidTex => board.UpdateTexture(vidTex)));
        }

        private void UserRemoved(CavrnusUser user)
        {
            if (streamHooks.TryGetValue(user, out var hook))
                hook?.Dispose();
        }
        
        private void OnDestroy()
        {
            listDisp?.Dispose();
        }
    }
}