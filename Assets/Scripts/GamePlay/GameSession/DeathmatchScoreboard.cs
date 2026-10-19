using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using UnityEngine;

namespace GamePlay
{
	// The server keeps the scores by connection and sends the whole table to the clients on every change.
	public class DeathmatchScoreboard
	{
		private readonly VSNetworkManager _networkManager;
		private readonly Dictionary<NetworkConnectionToClient, ScoreboardEntry> _entryByConnection =
			new Dictionary<NetworkConnectionToClient, ScoreboardEntry>();
		private readonly Dictionary<NetworkConnectionToClient, Texture2D> _avatarByConnection =
			new Dictionary<NetworkConnectionToClient, Texture2D>();

		private readonly Dictionary<int, Texture2D> _avatarByPlayerId = new Dictionary<int, Texture2D>();
		private readonly Subject<Unit> _changed = new Subject<Unit>();
		private ScoreboardEntry[] _entries = Array.Empty<ScoreboardEntry>();

		public IReadOnlyList<ScoreboardEntry> Entries => _entries;
		public Observable<Unit> Changed => _changed;

		private DeathmatchScoreboard(VSNetworkManager networkManager)
		{
			_networkManager = networkManager;
		}

		public static DeathmatchScoreboard Create(VSNetworkManager networkManager)
		{
			var scoreboard = new DeathmatchScoreboard(networkManager);

			networkManager.MessageReceived.OfMessageType<ScoreboardResponse>()
				.Subscribe(directedMessage => scoreboard.OnScoreboardResponse(directedMessage.Message.Entries))
				.AddTo(networkManager);
			networkManager.MessageReceived.OfMessageType<PlayerAvatarResponse>()
				.Subscribe(directedMessage => scoreboard.OnPlayerAvatarResponse(directedMessage.Message))
				.AddTo(networkManager);
			return scoreboard;
		}

		public Texture2D GetAvatar(int playerId)
		{
			return _avatarByPlayerId.GetValueOrDefault(playerId);
		}

		public void AddPlayer(NetworkConnectionToClient connection, string nickName, Texture2D avatar)
		{
			_entryByConnection[connection] = new ScoreboardEntry(connection.connectionId, nickName);
			_avatarByConnection[connection] = avatar;
			_networkManager.SendResponseToAll(new PlayerAvatarResponse(connection.connectionId, avatar));
			SendToAll();
		}

		public void RemovePlayer(NetworkConnectionToClient connection)
		{
			_avatarByConnection.Remove(connection);

			if (_entryByConnection.Remove(connection))
			{
				SendToAll();
			}
		}

		public void ChangeClass(NetworkConnectionToClient connection, GameClass gameClass)
		{
			Change(connection, entry => entry.WithGameClass(gameClass));
		}

		public void AddKill(NetworkConnectionToClient connection)
		{
			Change(connection, entry => entry.WithScore(entry.Kills + 1, entry.Deaths));
		}

		public void AddDeath(NetworkConnectionToClient connection)
		{
			Change(connection, entry => entry.WithScore(entry.Kills, entry.Deaths + 1));
		}

		public void ResetRound()
		{
			foreach (NetworkConnectionToClient connection in _entryByConnection.Keys.ToArray())
			{
				ScoreboardEntry entry = _entryByConnection[connection];
				_entryByConnection[connection] = new ScoreboardEntry(entry.PlayerId, entry.NickName);
			}

			SendToAll();
		}

		// Clients miss the broadcasts made while they are loading, so the full table is sent once they are ready.
		public void SendTo(NetworkConnectionToClient connection)
		{
			foreach ((NetworkConnectionToClient player, Texture2D avatar) in _avatarByConnection)
			{
				_networkManager.SendResponse(connection, new PlayerAvatarResponse(player.connectionId, avatar));
			}

			_networkManager.SendResponse(connection, new ScoreboardResponse(_entryByConnection.Values.ToArray()));
		}

		private void Change(NetworkConnectionToClient connection, Func<ScoreboardEntry, ScoreboardEntry> change)
		{
			if (!_entryByConnection.TryGetValue(connection, out ScoreboardEntry entry))
			{
				return;
			}

			_entryByConnection[connection] = change(entry);
			SendToAll();
		}

		private void SendToAll()
		{
			_networkManager.SendResponseToAll(new ScoreboardResponse(_entryByConnection.Values.ToArray()));
		}

		private void OnScoreboardResponse(ScoreboardEntry[] entries)
		{
			_entries = entries ?? Array.Empty<ScoreboardEntry>();

			foreach (int playerId in _avatarByPlayerId.Keys.ToArray())
			{
				if (_entries.All(entry => entry.PlayerId != playerId))
				{
					_avatarByPlayerId.Remove(playerId);
				}
			}

			_changed.OnNext(Unit.Default);
		}

		private void OnPlayerAvatarResponse(PlayerAvatarResponse response)
		{
			_avatarByPlayerId[response.PlayerId] = response.Avatar;
			_changed.OnNext(Unit.Default);
		}
	}
}
