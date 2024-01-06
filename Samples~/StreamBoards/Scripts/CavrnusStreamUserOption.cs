using System;
using Collab.Base.Collections;
using TMPro;
using UnityEngine;

namespace CavrnusSdk.StreamBoards
{
    public class CavrnusStreamUserOption : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI userName;

        private CavrnusUser user;
        private Action<CavrnusUser> onUserSelected;

        public void Setup(CavrnusUser user, Action<CavrnusUser> onUserSelected)
        {
            this.user = user;
            this.onUserSelected = onUserSelected;

            if (user == null) {

                userName.text = "Reset Stream";
                return;
            }

            userName.text = user.UserName.Translating(s => s).Value;
        }

        public void SelectUser()
        {
            onUserSelected?.Invoke(user);
        }
    }
}