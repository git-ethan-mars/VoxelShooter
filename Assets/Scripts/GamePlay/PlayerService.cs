using System;
using System.Collections.Generic;
using Data;
using Services;
namespace GamePlay
{
	public class PlayerService : IPlayerService
	{
		private readonly Dictionary<int, PlayerData> _playerDataById = new Dictionary<int, PlayerData>();

		public void AddPlayer(int playerId, PlayerData playerData)
		{
			if (!_playerDataById.TryAdd(playerId, playerData))
			{
				throw new InvalidOperationException($"Player with id {playerId} already exists.");
			}
		}

		public void RemovePlayer(int playerId)
		{
			_playerDataById.Remove(playerId);
		}

		public bool TryGetPlayerData(int playerId, out PlayerData playerData)
		{
			return _playerDataById.TryGetValue(playerId, out playerData);
		}
	}
}