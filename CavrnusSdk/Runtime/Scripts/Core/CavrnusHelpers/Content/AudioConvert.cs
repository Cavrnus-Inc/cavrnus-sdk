using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Collab.Base.Scheduler;
using Collab.Holo;
using Collab.Proxy;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityBase.Content
{
    public static class AudioConvert
    {
        public static Task<AudioClip> ConvertAudioClip(AudioAssetHoloStreamComponent a, IUnityScheduler sched, IProcessFeedback pf = null)
        {
            var tcs = new TaskCompletionSource<AudioClip>();
            sched.ExecCoRoutine(GetAudioRoutine(tcs, a));
            
            return tcs.Task;
        }
        
        private static IEnumerator GetAudioRoutine(TaskCompletionSource<AudioClip> tcs, AudioAssetHoloStreamComponent holo)
        {
            string tempAudioFileName;
            UnityWebRequest uwr;
            
            try {
                tempAudioFileName = CollabPaths.NewTemporaryFilePath($".{holo.Extension.ToLowerInvariant()}");
                File.WriteAllBytes(tempAudioFileName, holo.AudioData);
            
                var extension = GetAudioType(holo.FileType);
            
                uwr = UnityWebRequestMultimedia.GetAudioClip($"file:///{tempAudioFileName}", extension);
            }
            catch (Exception e) {
                tcs.SetException(e);
                yield break;
            }

            yield return uwr.SendWebRequest();

            try {
                var clip = DownloadHandlerAudioClip.GetContent(uwr);
                if (clip != null)
                {
                    clip.name = holo.FileName;

                    tcs.SetResult(clip);
                }

                if (File.Exists(tempAudioFileName))
                    File.Delete(tempAudioFileName);
            }
            catch (Exception e) {
                tcs.SetException(e);
            }
        }
        
        private static AudioType GetAudioType(AudioAssetHoloStreamComponent.FileTypeEnum fileType)
        {
            return fileType switch {
                AudioAssetHoloStreamComponent.FileTypeEnum.Mp3  => AudioType.MPEG,
                AudioAssetHoloStreamComponent.FileTypeEnum.Ogg  => AudioType.OGGVORBIS,
                AudioAssetHoloStreamComponent.FileTypeEnum.Wav  => AudioType.WAV,
                AudioAssetHoloStreamComponent.FileTypeEnum.Aiff => AudioType.AIFF,
                _                                               => AudioType.UNKNOWN
            };
        }
    }
}