﻿using System;
using System.Collections.Generic;
using CavrnusSdk.API;
using UnityEngine;

namespace CavrnusSdk.CollaborationExamples
{
    public class CavrnusSpaceLevelData : ScriptableObject
    {
        [Serializable]
        public class SpaceLevelInfo
        {
            public string Tag;
            public string SpaceDisplayName;
            public string CavrnusSpaceJoinId;
            public bool IncludeRTC = true;
        }

        public List<SpaceLevelInfo> Levels => levels;
        [SerializeField] private List<SpaceLevelInfo> levels;

        public void LoadLevel(CavrnusSpaceConnection sc, SpaceLevelInfo data, Action onSpaceLevelLoaded = null)
        {
            if (data == null) {
                Debug.Log($"{typeof(SpaceLevelInfo)} is null!");
                return;
            }

            sc?.ExitSpace();
            CavrnusFunctionLibrary.JoinSpaceWithOptions(data.CavrnusSpaceJoinId, new CavrnusSpaceConnectionConfig {
                Tag = data.Tag,
                IncludeRtc = data.IncludeRTC,
            }, spaceConn => { 
                Debug.Log($"Joined new space!: {data.CavrnusSpaceJoinId}");
                
                onSpaceLevelLoaded?.Invoke();
            }, onFailure => { });
        }
    }
}