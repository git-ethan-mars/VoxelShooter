using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Networking
{
	public interface IPlayerDataLoader
	{
		string GetPlayerNickName();
		UniTask<Texture2D> GetPlayerAvatarAsync();
	}
}