using System.Collections;
using CavrnusCore;
using UnityEngine;

namespace CavrnusSdk.PlatformPermissions
{
    public static class PlatformPermissionsRequestHelper
    {
        public static void RequestPermissions(bool disableVoice, bool disableVideo)
        {
            CavrnusStatics.Scheduler.ExecCoRoutine(RequestPermissionsRoutine(disableVoice, disableVideo));
        }
        
        // This fixes iOS permissions not prompting
        private static IEnumerator RequestPermissionsRoutine(bool disableAudio, bool disableVideo)
        {
            if (disableAudio && disableVideo)
            {
                Debug.Log("No permissions requested: Both audio and video are disabled");
                yield break;
            }

            UserAuthorization userAuthorization = 0;

            if (!disableAudio)
                userAuthorization |= UserAuthorization.Microphone;

            if (!disableVideo)
                userAuthorization |= UserAuthorization.WebCam;

            yield return Application.RequestUserAuthorization(userAuthorization);
        }
    }
}