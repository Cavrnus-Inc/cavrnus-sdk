using System;
using System.Collections.Generic;
using UnityEngine;

namespace CavrnusSdk.MultiplayerGame
{
    [CreateAssetMenu(fileName = "CavrnusEmoteCollection", menuName = "Cavrnus/Emote Collection", order = 1)]
    public class CavrnusEmoteCollection : ScriptableObject
    {
        [Serializable]
        public class SerializedEmoteData
        {
            public string DisplayName;
            public AnimationClip Animation;
        }

        public List<SerializedEmoteData> EmoteData;
    }
}