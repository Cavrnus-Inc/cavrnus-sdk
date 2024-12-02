using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    [CreateAssetMenu(fileName = "NewAvatarTypesCollection", menuName = "Cavrnus/Avatar Types", order = 1)]
    public class CavrnusAvatarTypes : ScriptableObject
    {
        [Serializable]
        public class AvatarType
        {
            public string Name;
            public GameObject Prefab;
            public Avatar Avatar;
        }

        public List<AvatarType> AvatarTypes;
    }
}