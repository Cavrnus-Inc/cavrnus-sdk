using System.Threading.Tasks;
using CavrnusSdk.API;
using Collab.RtcCommon;
using UnityEngine;

namespace CavrnusCore
{
    internal static class CavrnusRtcContextHelpers
    {
        internal static async Task<RtcContext> CreateRtcContext(CavrnusSpaceConnectionConfig config)
        {
#if UNITY_VISIONOS
            return await WaitForMicrophoneSetupAsync(config);
#else
            return await CreateContextAsync(config);
#endif
        }

        private static async Task<RtcContext> CreateContextAsync(CavrnusSpaceConnectionConfig config)
        {
            await Task.Yield();

            var input = RtcInputSource.FromJson("");
            var output = RtcOutputSink.FromJson("");
            var vidInput = RtcInputSource.FromJson("");

            RtcModeEnum sendMode;
            RtcModeEnum recvMode;

            // If SpatialConnector A/V settings are set then those have priority
            if (CavrnusStatics.CavrnusSettings.DisableVideo && CavrnusStatics.CavrnusSettings.DisableVoice)
            {
                sendMode = RtcModeEnum.None;
                recvMode = RtcModeEnum.None;

            }
            else if (CavrnusStatics.CavrnusSettings.DisableVideo)
            {
                sendMode = RtcModeEnum.AudioOnly;
                recvMode = RtcModeEnum.AudioOnly;
            }
            else if (CavrnusStatics.CavrnusSettings.DisableVoice)
            {
                sendMode = RtcModeEnum.Video;
                recvMode = RtcModeEnum.Video;
            }
            else
            {
                sendMode = config.IncludeRtc ? RtcModeEnum.AudioVideo : RtcModeEnum.None;
                recvMode = config.IncludeRtc ? RtcModeEnum.AudioVideo : RtcModeEnum.None;
            }

            var ctx = new RtcContext(CavrnusStatics.RtcSystem, CavrnusStatics.Scheduler.BaseScheduler);
            ctx.Initialize(input, output, vidInput, sendMode, recvMode);

            return ctx;
        }

        private static GameObject audioSourceGameObject;
        private static async Task<RtcContext> WaitForMicrophoneSetupAsync(CavrnusSpaceConnectionConfig config)
        {
            await Task.Yield();

            if (audioSourceGameObject != null)
                Object.Destroy(audioSourceGameObject);
            
            audioSourceGameObject = new GameObject("MicrophoneTester");
            var audioSource = audioSourceGameObject.AddComponent<AudioSource>();

            // Wait until at least one microphone device is available
            while (Microphone.devices.Length == 0)
            {
                await Task.Delay(100);
            }

            var retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                var startTime = Time.time;
                
                audioSource.clip = Microphone.Start(null, true, 10, 44100);
                while (!(Microphone.GetPosition(null) > 0))
                {
                    if (Time.time - startTime > 5f) // Timeout for this attempt
                    {
                        Debug.LogWarning($"CAVRNUS: Microphone start timed out. Retrying ({retryCount + 1}/{maxRetries})...");
                        retryCount++;
                        break;
                    }

                    await Task.Delay(100);
                }

                // Microphone is initialized AND working now...
                if (Microphone.IsRecording(null))
                {
                    Object.Destroy(audioSourceGameObject);
                    return await CreateContextAsync(config);
                }
            }

            Debug.LogWarning("CAVRNUS: Failed to start microphone after maximum retries. Joining space...");
            Object.Destroy(audioSourceGameObject);
            return null;
        }
    }
}
