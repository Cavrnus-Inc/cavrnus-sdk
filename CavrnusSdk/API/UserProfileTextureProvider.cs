using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Scheduler;
using Collab.Holo;
using Collab.LiveRoomSystem;
using Collab.Proxy.Comm;
using Collab.Proxy.Comm.LocalTypes;
using Collab.Proxy.Comm.RestApi;
using Google.Protobuf;
using UnityBase;
using UnityBase.Content;
using UnityEngine;

namespace CavrnusSdk
{
	public static class TextureSettingExtensions
	{
		public static IReadonlySetting<Vector2> SizeOfTextureSetting(this IReadonlySetting<TextureWithUVs> intex, Vector2 ifNull)
		{
			var r = new CachedValueTranslatingSetting<TextureWithUVs, Vector2>();
			r.Initialize(intex,
				tex =>
				{
					if (UnityBase.HelperFunctions.NullOrDestroyed(tex?.Texture))
						return ifNull;
					return new Vector2(tex.Texture.width, tex.Texture.height);
				});
			return r;
		}
		public static IReadonlySetting<Vector2> SizeOfTextureSetting(this IReadonlySetting<Texture> intex, Vector2 ifNull)
		{
			var r = new CachedValueTranslatingSetting<Texture, Vector2>();
			r.Initialize(intex,
				tex =>
				{
					if (UnityBase.HelperFunctions.NullOrDestroyed(tex))
						return ifNull;
					return new Vector2(tex.width, tex.height);
				});
			return r;
		}
	}

	public class UserProfileTextureProvider
	{
		public IReadonlySetting<TextureWithUVs> UsersMenuTex => cameraStreamOrProfileTexture;

		private IReadonlySetting<bool> StreamingToUsersMenu { get; }
		private IReadonlySetting<TextureWithUVs> overridingUsersMenuStreamPic;
		private IReadonlySetting<TextureWithUVs> cameraStreamOrProfileTexture;
		private IRegistrationSet<TextureWithUVs, int> combinedUsersMenuTextureRegSet { get; }
		private IRegistrationHook<TextureWithUVs, int> usersMenuVideoTextureHook { get; }

		private ISessionCommunicationUser user;

		private string lastProfileUrl;

		private ISetting<TextureWithUVs> profilePic = new Setting<TextureWithUVs>();

		private RestApiEndpoint auth;
		private INetworkRequestImplementation nri;
		private IUnityScheduler scheduler;

		public UserProfileTextureProvider(ISessionCommunicationUser user, UserVideoTextureProvider videoTextureProvider, RoomSystem rs, INetworkRequestImplementation nri, IUnityScheduler scheduler)
		{
			this.nri = nri;
			this.scheduler = scheduler;
			auth = rs.Comm.ServerEndpoint;

			this.user = user;

			this.usersMenuVideoTextureHook = videoTextureProvider.CreateHook();
			StreamingToUsersMenu = user.PresentationStreamActive.OrSetting(user.CameraStreamActive);

			overridingUsersMenuStreamPic = new MultiTranslatingSetting<bool, TextureWithUVs, TextureWithUVs>(StreamingToUsersMenu, usersMenuVideoTextureHook.Data, (useStream, vidTex) => useStream ? vidTex : null);
			cameraStreamOrProfileTexture = new MultiTranslatingSetting<TextureWithUVs, TextureWithUVs, TextureWithUVs>(overridingUsersMenuStreamPic, profilePic, (stream, profile) => stream ?? profile);

			this.combinedUsersMenuTextureRegSet = RegistrationSetFactory<TextureWithUVs, int>.CreateRegistrationSet(cameraStreamOrProfileTexture, (quals) => quals.Min());
			this.combinedUsersMenuTextureRegSet.AggregatePayload.ChangedEvent += AggregateUsersMenuPayloadOnChangedEvent;
			this.combinedUsersMenuTextureRegSet.Live.ChangedEvent += LiveUsersMenuOnChangedEvent;
			
			user.User.UserData.ChangedEvent += UserOnUserMetadataUpdated;

			UserOnUserMetadataUpdated(user.User.UserData.Value, null);
		}

		private void LiveUsersMenuOnChangedEvent(bool newvalue, bool oldvalue)
		{
			if (newvalue)
			{
				this.usersMenuVideoTextureHook.Request(this.combinedUsersMenuTextureRegSet.AggregatePayload.Value);
			}
			else
			{
				this.usersMenuVideoTextureHook.Release();
			}
		}

		private void AggregateUsersMenuPayloadOnChangedEvent(int newvalue, int oldvalue)
		{
			this.usersMenuVideoTextureHook.Request(newvalue);
		}

		public IRegistrationHook<TextureWithUVs, int> CreateCombinedUsersMenuTextureHook()
		{
			return this.combinedUsersMenuTextureRegSet.CreateHook();
		}
		
		private async void UserOnUserMetadataUpdated(UserProfileInfoLive newData, UserProfileInfoLive oldData)
		{
			var newProfileUrl = auth.GetUriForRemoteContent(user.User.UserData.Value.PictureUrl ?? "")?.ToString();
			if (newProfileUrl != lastProfileUrl)
			{
				lastProfileUrl = newProfileUrl;
				profilePic.Value = new TextureWithUVs(null);

				try
				{
					if (newProfileUrl != lastProfileUrl)
						return;

					var tex = await DownloadProfilePic(newProfileUrl);

					if (tex != null)
						profilePic.Value = new TextureWithUVs(tex);
					else
						profilePic.Value = null;
				}
				catch (OperationCanceledException)
				{
					profilePic.Value = null;
				}
				catch (Exception e)
				{
					DebugOutput.Log("Failed to acquire profile picture url '" + lastProfileUrl + "': " + e);
					profilePic.Value = null;
				}
			}
		}

		private async Task<Texture2D> DownloadProfilePic(string url)
		{
			var res = await Downloader.DoDownloadToProcess(nri, url, async (nrs, pf2) =>
			{
				string extension = null;
				switch (nrs.ContentType)
				{
					case null:
					case "application/octet-stream":
					default:
						extension = Path.GetExtension(url).ToLowerInvariant();
						if (String.IsNullOrWhiteSpace(extension))
						{
							extension = ".png"; // eh.
						}
						break;
					case "image/bmp":
						extension = ".bmp";
						break;
					case "image/gif":
						extension = ".gif";
						break;
					case "image/jpeg":
						extension = ".jpg";
						break;
					case "image/png":
						extension = ".png";
						break;
				}

				if (string.IsNullOrEmpty(extension))
				{
					DebugOutput.Info($"Image at url {url} has no extension and cannot therefore be loaded.");
					return null;
				}

				//DebugOutput.Info($"WICS: {Path.GetFileNameWithoutExtension(k)}, {nrs.ContentLength} content-length bytes.");

				var tex = await ConvertStreamToImage(nrs.Data, nrs.ContentLength, extension, Path.GetFileNameWithoutExtension(url), true);
				return tex;
			});

			return res;
		}

		private async Task<Texture2D> ConvertStreamToImage(Stream dataStream, long dataLength, string extension, string imageName, bool checkRotationMetadata)
		{
			FilterMode uFilter = FilterMode.Trilinear;

			bool isLinear = false;
			bool canCompress = true;
			var tex = (Texture2D)await TextureConvert.ConvertTextureFromData(extension, imageName, dataStream, dataLength, uFilter, scheduler, isLinear, canCompress, checkRotationMetadata);
			return tex;
		}

		public void Shutdown()
		{
			this.usersMenuVideoTextureHook.Release();
			user.User.UserData.ChangedEvent -= UserOnUserMetadataUpdated;
		}
	}
}
