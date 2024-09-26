using System;
using Collab.Proxy.Prop.JournalInterop;
using System.Collections;
using CavrnusCore;
using UnityEngine;
using UnityEngine.Networking;

namespace CavrnusSdk.API
{
    public static class CavrnusShortcutsLibrary
    {
		public static bool GetUserSpeaking(this CavrnusUser user)
		{
			return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Speaking);
		}

		public static IDisposable BindUserSpeaking(this CavrnusUser user, Action<bool> onSpeakingChanged)
        {
            return user.SpaceConnection.BindBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Speaking, onSpeakingChanged);
        }

		public static bool GetUserMuted(this CavrnusUser user)
        {
            return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Muted);
        }

		public static IDisposable BindUserMuted(this CavrnusUser user, Action<bool> onMutedChanged)
		{
			return user.SpaceConnection.BindBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Muted, onMutedChanged);
		}

		public static bool GetUserStreaming(this CavrnusUser user)
		{
			return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Streaming);
		}

		public static IDisposable BindUserStreaming(this CavrnusUser user, Action<bool> onStreamingChanged)
		{
			return user.SpaceConnection.BindBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Streaming, onStreamingChanged);
		}

		public static string GetUserName(this CavrnusUser user)
		{
			return user.SpaceConnection.GetStringPropertyValue(user.ContainerId, UserPropertyDefs.Users_Name);
		}

		public static IDisposable BindUserName(this CavrnusUser user, Action<string> onNameChanged)
		{
			return user.SpaceConnection.BindStringPropertyValue(user.ContainerId, UserPropertyDefs.Users_Name, onNameChanged);
		}       

        public static IDisposable BindProfilePic(this CavrnusUser user, Action<Sprite> onProfilePicChanged)
        {
            var disp = user.SpaceConnection.BindStringPropertyValue(user.ContainerId, UserPropertyDefs.Users_Profile_ProfilePicture, (pp) => {
	            CavrnusStatics.Scheduler.ExecCoRoutine(LoadProfilePic(pp, onProfilePicChanged));
            });
            return disp;
        }

        public static IEnumerator LoadProfilePic(string url, Action<Sprite> onProfilePicChanged)
        {
	        if (url == null) yield break;
	        
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                var sprite = Sprite.Create(myTexture as Texture2D, new Rect(Vector2.zero, new Vector2(myTexture.width, myTexture.height)), Vector2.zero);

                onProfilePicChanged(sprite);
            }
        }

        public static void PostTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector3 pos, Vector3 rot, Vector3 scl)
        {
            spaceConn.PostTransformPropertyUpdate(containerName, propertyName, new CavrnusTransformData(pos, rot, scl));
        }

        public static void PostTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Transform transform)
        {
            spaceConn.PostTransformPropertyUpdate(containerName, propertyName, new CavrnusTransformData(transform.localPosition, transform.localEulerAngles, transform.localScale));
        }

		public static string SpawnObject(this CavrnusSpaceConnection spaceConn, string uniqueId, Vector3 pos, Action<CavrnusSpawnedObject, GameObject> onSpawnComplete = null)
		{
			string instanceContainerId = spaceConn.SpawnObject(uniqueId, onSpawnComplete);

			spaceConn.PostTransformPropertyUpdate(instanceContainerId, "Transform", pos, Vector3.zero, Vector3.one);

			return instanceContainerId;
		}

		public static string SpawnObject(this CavrnusSpaceConnection spaceConn, string uniqueId, Vector3 pos, Vector3 rot, Vector3 scale, Action<CavrnusSpawnedObject, GameObject> onSpawnComplete = null)
        {
			string instanceContainerId = spaceConn.SpawnObject(uniqueId, onSpawnComplete);

			spaceConn.PostTransformPropertyUpdate(instanceContainerId, "Transform", pos, rot, scale);

            return instanceContainerId;
		}
    }
}
