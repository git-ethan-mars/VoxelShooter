using System.Globalization;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;
namespace Networking
{
	public class LocalPlayerDataLoader : IPlayerDataLoader
	{
		private const string AvatarPath = "Sprites/steam-question-mark";
		
		private readonly IAssetProvider _assets;

		public LocalPlayerDataLoader(IAssetProvider assets)
		{
			_assets = assets;
		}
		
		public string GetPlayerNickName()
		{
			return Random.value.ToString(CultureInfo.InvariantCulture);
		}

		public UniTask<Texture2D> GetPlayerAvatarAsync()
		{
			return UniTask.FromResult(_assets.Load<Texture2D>(AvatarPath));
		}
	}
}