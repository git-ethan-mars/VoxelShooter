using GamePlay;
using GamePlay.Data;
using Mirror;

namespace Networking
{
	public class PlayerDataSynchronization : NetworkBehaviour, IPlayerData
	{
		[SyncVar] public string nickName;
		[SyncVar] public GameClass gameClass;
		public string NickName => nickName;
		public GameClass GameClass => gameClass;
	}
}