using Collab.Base.Collections;
using Collab.LiveRoomSystem;
using Collab.Proxy.Comm;
using Collab.Proxy.Comm.LocalTypes;
using System;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk
{
	public class CavrnusUser : IDisposable
	{
		public bool IsLocalUser;

		public string ContainerId;

		public IReadonlySetting<string> UserName;
		public IReadonlySetting<bool> IsSpeaking;
		public IReadonlySetting<bool> IsMuted;
		public IReadonlySetting<float> SpeakingVolume;
		public IReadonlySetting<Sprite> ProfileAndVideoTexture;
		public IReadonlySetting<Sprite> VideoTexture;

		public IReadonlySetting<bool> IsStreaming;

		private ISessionCommunicationUser user;
		private IRegistrationHook<TextureWithUVs, int> userVidHook;

		public CavrnusUser(ISessionCommunicationUser user, RoomSystem rs)
		{
			this.user = user;

			ContainerId = $"/users/{user.ConnectionId}";

			IsLocalUser = user is ISessionCommunicationLocalUser;

			UserName = user.User.UserData.Translating(ud => ud.GetVisibleName());

			SpeakingVolume = (user as ISessionCommunicationRemoteUser)?.Rtc?.CurrentReceivingVolume ??
			                 (user as ISessionCommunicationLocalUser)?.Rtc?.CurrentTransmittingVolume;

			IsSpeaking = SpeakingVolume.Translating(v => v > .005f);

			IsMuted = user.Muted;

			UserVideoTextureProvider vidProvider = new UserVideoTextureProvider(user);
			UserProfileTextureProvider profileProvider =
				new UserProfileTextureProvider(user, vidProvider, rs, CavrnusHelpers.NetworkRequestImpl,
				                               CavrnusHelpers.Scheduler);

			userVidHook = profileProvider.CreateCombinedUsersMenuTextureHook();
			userVidHook.Request(1);

			ProfileAndVideoTexture = profileProvider.UsersMenuTex.Translating(tuv => {
				if (tuv?.Texture == null) {
					
					return null;
				}
				else {
					var sprite = Sprite.Create(tuv.Texture as Texture2D,
					                           new Rect(Vector2.zero,
					                                    new Vector2(tuv.Texture.width * tuv.UVRect.width,
					                                                tuv.Texture.height * tuv.UVRect.height)),
					                           Vector2.zero);
					return sprite;
				}
			});
			
			VideoTexture = profileProvider.UsersStream.Translating(tuv => {
				if (tuv?.Texture == null) {
					
					return null;
				}
				else {
					var sprite = Sprite.Create(tuv.Texture as Texture2D,
					                           new Rect(Vector2.zero,
					                                    new Vector2(tuv.Texture.width * tuv.UVRect.width,
					                                                tuv.Texture.height * tuv.UVRect.height)),
					                           Vector2.zero);
					return sprite;
				}
			});
			
			IsStreaming = profileProvider.StreamingToUsersMenu;
		}

		public IDisposable BindLatestCoPresence(Action<CoPresenceLive> act) { return user.LatestCoPresence.Bind(act); }

		public void Dispose() { userVidHook.Release(); }
	}
}