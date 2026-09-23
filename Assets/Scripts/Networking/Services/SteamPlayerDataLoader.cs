using System;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
namespace Networking
{
	public class SteamPlayerDataLoader : IPlayerDataLoader, IDisposable
	{
		private readonly Callback<AvatarImageLoaded_t> _avatarLoaded;
		private readonly UniTaskCompletionSource<Texture2D> _taskCancellationSource;

		public SteamPlayerDataLoader()
		{
			_avatarLoaded = Callback<AvatarImageLoaded_t>.Create(OnPlayerAvatarDownloaded);
			_taskCancellationSource = new UniTaskCompletionSource<Texture2D>();
		}

		public string GetPlayerNickName()
		{
			return SteamFriends.GetPersonaName();
		}

		public async UniTask<Texture2D> GetPlayerAvatarAsync()
		{
			CSteamID steamID = SteamUser.GetSteamID();
			int imageHandle = SteamFriends.GetLargeFriendAvatar(steamID);

			if (imageHandle > 0)
			{
				return ReadPlayerAvatar(imageHandle);
			}
			
			return await _taskCancellationSource.Task;
		}
		
		private Texture2D ReadPlayerAvatar(int imageHandle)
		{
			if (!SteamUtils.GetImageSize(imageHandle, out uint avatarWidth, out uint avatarHeight))
			{
				Debug.Log("Can't get avatar size");
			}

			var avatarSizeInBytes = (int)(avatarWidth * avatarHeight * 4);
			var avatarBuffer = new byte[avatarSizeInBytes];
			if (!SteamUtils.GetImageRGBA(imageHandle, avatarBuffer, avatarSizeInBytes))
			{
				Debug.Log("Can't download avatar");
			}
			var texture = new Texture2D((int)avatarWidth, (int)avatarHeight, TextureFormat.RGBA32, false, true);
			texture.LoadRawTextureData(avatarBuffer);
			texture.Apply();
			return texture;
		}
		
		private void OnPlayerAvatarDownloaded(AvatarImageLoaded_t avatarImageResult)
		{
			var texture = ReadPlayerAvatar(avatarImageResult.m_iImage);
			_taskCancellationSource.TrySetResult(texture);
		}

		public void Dispose()
		{
			_avatarLoaded?.Dispose();
		}
	}
}