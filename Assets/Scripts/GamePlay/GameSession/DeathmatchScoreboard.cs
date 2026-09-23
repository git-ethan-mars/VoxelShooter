using Data;
using Mirror;
using Networking.Core;
using ObservableCollections;
using UnityEngine;

namespace GamePlay
{
	public class DeathmatchScoreboard
	{
		private readonly SyncReactiveDictionary<NetworkConnectionToClient, DeathMatchPlayerData> _playerDataById =
			new SyncReactiveDictionary<NetworkConnectionToClient, DeathMatchPlayerData>();

		public ObservableDictionary<NetworkConnectionToClient, DeathMatchPlayerData> PlayerDataById => _playerDataById;

		public DeathMatchPlayerData AddPlayer(NetworkConnectionToClient connection, string nickName, Texture2D avatar)
		{
			_playerDataById[connection] = new DeathMatchPlayerData(nickName, avatar);
			return _playerDataById[connection];
		}

		public void RemovePlayer(NetworkConnectionToClient connection)
		{
			_playerDataById.Remove(connection);
		}

		public void ChangeClass(NetworkConnectionToClient connection, GameClass gameClass)
		{
			var playerData = _playerDataById[connection];
			_playerDataById[connection] = playerData.WithGameClass(gameClass);
		}
	}
}
