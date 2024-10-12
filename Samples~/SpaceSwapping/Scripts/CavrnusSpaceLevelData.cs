using System;
using System.Collections;
using System.Collections.Generic;
using CavrnusCore;
using CavrnusSdk.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CavrnusSdk.CollaborationExamples
{
    [CreateAssetMenu(fileName = "CavrnusSpaceLevelData", menuName = "Cavrnus/Samples/SpaceLevelData", order = 0)]
    public class CavrnusSpaceLevelData : ScriptableObject
    {
        [Serializable]
        public class SpaceLevelInfo
        {
            public string Tag;
            public string SpaceDisplayName;
            public string CavrnusSpaceJoinId;
            public SceneAsset Scene;
            public bool IncludeRtc;
        }

        public List<SpaceLevelInfo> Levels => levels;
        [SerializeField] private List<SpaceLevelInfo> levels;

        private bool cavrnusSpaceJoined;
        private bool unitySceneLoaded;

        public void LoadLevel(CavrnusSpaceConnection sc, SpaceLevelInfo data, Action onSpaceLevelLoaded = null)
        {
            cavrnusSpaceJoined = false;
            unitySceneLoaded = false;
            
            if (data == null) {
                Debug.Log($"{typeof(SpaceLevelInfo)} is null!");
                return;
            }

            var loadOp = SceneManager.LoadSceneAsync(data.Scene.name);
            CavrnusStatics.Scheduler.StartCoroutine(WaitForSceneLoad(loadOp, () => {
                unitySceneLoaded = true;
                CheckSuccessfulJoin();
                
                Debug.Log($"Unity scene loaded!: {data.Scene.name}");
            }));
            
            sc?.ExitSpace();
            CavrnusFunctionLibrary.JoinSpaceWithOptions(data.CavrnusSpaceJoinId, new SpaceConnectionConfig {
                Tag = data.Tag,
            }, spaceConn => { 
                cavrnusSpaceJoined = true;
                CheckSuccessfulJoin();
                
                Debug.Log($"Joined new space!: {data.CavrnusSpaceJoinId}");
            });
            
            return;

            void CheckSuccessfulJoin()
            {
                if (cavrnusSpaceJoined && unitySceneLoaded) {
                    onSpaceLevelLoaded?.Invoke();
                }
            }
        }
        
        private static IEnumerator WaitForSceneLoad(AsyncOperation asyncOperation, Action onSceneLoaded)
        {
            while (!asyncOperation.isDone)
            {
                Debug.LogWarning($"Level load process: {asyncOperation.progress}");
                
                if (asyncOperation.progress >= 0.9f && !asyncOperation.allowSceneActivation)
                    asyncOperation.allowSceneActivation = true;

                yield return null;
            }
    
            onSceneLoaded?.Invoke();
        }
    }
}