using System;
using Collab.Proxy.Prop.JournalInterop;
using System.Collections;
using CavrnusCore;
using Collab.Base.Collections;
using Collab.Base.Core;
using Newtonsoft.Json.Linq;
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

		public static void DeleteLocalUserMetadataString(this CavrnusSpaceConnection user, string key)
		{
			CavrnusPropertyHelpers.DeleteLocalUserMetadataByKey(key);
		}
		
		public static void UpdateLocalUserMetadataString(this CavrnusUser user, string key, string value)
		{
			CavrnusPropertyHelpers.UpdateLocalUserMetadata(key, value);
		}
		
		public static void UpdateLocalUserMetadataJson(this CavrnusSpaceConnection spaceConnection, string key, JObject jValue)
		{
			CavrnusPropertyHelpers.UpdateLocalUserMetadata(key, jValue.ToString());
		}

		public static IDisposable BindToLocalUserMetadataString(this CavrnusUser user, string key, Action<string> onMetadataChanged)
		{
			IDisposable internalBinding = null;
			
			var userBind = user.ContainerIdSetting.Bind(containerId => {
				internalBinding?.Dispose();
				internalBinding = null;
				
				if (containerId == null) 
					return;

				internalBinding = user.SpaceConnection.BindStringPropertyValue($"{containerId}/meta/", key, onMetadataChanged);
			});

			return new DelegatedDisposalHelper(() => {
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
		}
		
		public static IDisposable BindToLocalUserMetadataJson(this CavrnusUser user, string key, Action<JObject> onMetadataChanged)
		{
			IDisposable internalBinding = null;

			var userBind = user.ContainerIdSetting.Bind(containerId =>
			{
				internalBinding?.Dispose();
				internalBinding = null;

				if (containerId == null)
					return;

				internalBinding = user.SpaceConnection.BindJsonPropertyValue($"{containerId}/meta/", key, propValue => {
					onMetadataChanged?.Invoke(propValue);
				});
			});

			return new DelegatedDisposalHelper(() =>
			{
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
		}

		public static IDisposable BindUserSpeaking(this CavrnusUser user, Action<bool> onSpeakingChanged)
		{
			IDisposable internalBinding = null;
			var userBind = user.ContainerIdSetting.Bind(containerId => {
				internalBinding?.Dispose();
				internalBinding = null;
				
				if (containerId == null) 
					return;
				
				internalBinding = user.SpaceConnection.BindBoolPropertyValue(containerId, UserPropertyDefs.User_Speaking, onSpeakingChanged);
			});

			return new DelegatedDisposalHelper(() => {
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
        }

		public static bool GetUserMuted(this CavrnusUser user)
        {
            return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Muted);
        }

		public static IDisposable BindUserMuted(this CavrnusUser user, Action<bool> onMutedChanged)
		{
			IDisposable internalBinding = null;
			var userBind = user.ContainerIdSetting.Bind(containerId => {
				internalBinding?.Dispose();
				internalBinding = null;
				
				if (containerId == null) 
					return;
				
				internalBinding = user.SpaceConnection.BindBoolPropertyValue(containerId, UserPropertyDefs.User_Muted, onMutedChanged);
			});

			return new DelegatedDisposalHelper(() => {
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
		}

		public static bool GetUserStreaming(this CavrnusUser user)
		{
			return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Streaming);
		}

		public static IDisposable BindUserStreaming(this CavrnusUser user, Action<bool> onStreamingChanged)
		{
			IDisposable internalBinding = null;
			var userBind = user.ContainerIdSetting.Bind(containerId => {
				internalBinding?.Dispose();
				internalBinding = null;
				
				if (containerId == null) 
					return;
				
				internalBinding = user.SpaceConnection.BindBoolPropertyValue(containerId, UserPropertyDefs.User_Streaming, onStreamingChanged);
			});

			return new DelegatedDisposalHelper(() => {
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
		}

		public static string GetUserName(this CavrnusUser user)
		{
			return user.SpaceConnection.GetStringPropertyValue(user.ContainerId, UserPropertyDefs.Users_Name);
		}

		public static IDisposable BindUserName(this CavrnusUser user, Action<string> onNameChanged)
		{
			IDisposable internalBinding = null;
			var userBind = user.ContainerIdSetting.Bind(containerId => {
				internalBinding?.Dispose();
				internalBinding = null;
				
				if (containerId == null) 
					return;
				
				internalBinding = user.SpaceConnection.BindStringPropertyValue(containerId, UserPropertyDefs.Users_Name, onNameChanged);
			});

			return new DelegatedDisposalHelper(() => {
				userBind?.Dispose();
				internalBinding?.Dispose();
			});
		}       

        public static IDisposable BindProfilePic(this CavrnusUser user, Action<Sprite> onProfilePicChanged)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;

		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindStringPropertyValue(containerId, UserPropertyDefs.Users_Profile_ProfilePicture, (pp) => {
			        CavrnusStatics.Scheduler.ExecCoRoutine(LoadProfilePic(pp, onProfilePicChanged));
		        });
	        });

	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
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

        public static IDisposable BindBoolPropertyValue(this CavrnusUser user, string propertyName, Action<bool> onPropertyUpdated)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;
				
		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindBoolPropertyValue(containerId, propertyName, onPropertyUpdated);
	        });

	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
        }
        
        public static IDisposable BindStringPropertyValue(this CavrnusUser user, string propertyName, Action<string> onPropertyUpdated)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;
				
		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindStringPropertyValue(containerId, propertyName, onPropertyUpdated);
	        });

	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
        }
        
        public static IDisposable BindColorPropertyValue(this CavrnusUser user, string propertyName, Action<Color> onPropertyUpdated)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;
				
		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindColorPropertyValue(containerId, propertyName, onPropertyUpdated);
	        });

	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
        }
        
        public static IDisposable BindFloatPropertyValue(this CavrnusUser user, string propertyName, Action<float> onPropertyUpdated)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;
				
		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindFloatPropertyValue(containerId, propertyName, onPropertyUpdated);
	        });
	        
	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
        }
        
        public static IDisposable BindTransformPropertyValue(this CavrnusUser user, string propertyName, Action<CavrnusTransformData> onPropertyUpdated)
        {
	        IDisposable internalBinding = null;
	        var userBind = user.ContainerIdSetting.Bind(containerId => {
		        internalBinding?.Dispose();
		        internalBinding = null;
				
		        if (containerId == null) 
			        return;
				
		        internalBinding = user.SpaceConnection.BindTransformPropertyValue(containerId, propertyName, onPropertyUpdated);
	        });
	        
	        return new DelegatedDisposalHelper(() => {
		        userBind?.Dispose();
		        internalBinding?.Dispose();
	        });
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
