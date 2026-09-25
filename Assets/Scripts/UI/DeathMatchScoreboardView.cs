using System;
using System.Collections.Generic;
using Data;
using GamePlay;
using Mirror;
using ObservableCollections;
using UnityEngine;

namespace UI
{
	public class DeathMatchScoreboardView : ListView<DeathMatchScoreboardRecord>
	{
		private DeathmatchScoreboard _scoreboard;
		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

		public void Initialize(DeathmatchScoreboard scoreboard)
		{
			_scoreboard = scoreboard;
			_scoreboard.PlayerDataById.CollectionChanged += OnCollectionChanged;
		}

		private void OnDestroy()
		{
			_scoreboard.PlayerDataById.CollectionChanged -= OnCollectionChanged;
		}

		private void OnCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<NetworkConnectionToClient, DeathMatchPlayerData>> e)
		{
			if (Items.Count < _scoreboard.PlayerDataById.Count)
			{
				int difference = _scoreboard.PlayerDataById.Count - Items.Count;
				for (int i = 0; i < difference; i++)
				{
					SpawnElement();
				}
			}
			else
			{
				for (int i = _scoreboard.PlayerDataById.Count; i < Items.Count; i++)
				{
					DespawnElement(Items[i]);
				}
			}

			int index = 0;

			foreach ((_, DeathMatchPlayerData playerData) in _scoreboard.PlayerDataById)
			{
				Items[index].NickName.SetText(playerData.NickName);
				Items[index].KillCount.SetText(playerData.Kills.ToString());
				Items[index].DeathCount.SetText(playerData.Deaths.ToString());
				Items[index].ClassName.SetText(playerData.GameClass.ToString());
				Items[index].Avatar.texture = playerData.Avatar;
				index++;
			}
		}
	}
}
