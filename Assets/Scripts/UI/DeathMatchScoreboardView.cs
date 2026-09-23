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
		[field:SerializeField] public CanvasGroup CanvasGroup { get; private set; }

		private DeathmatchScoreboard _scoreboard;

		public void Initialize(DeathmatchScoreboard scoreboard)
		{
			_scoreboard = scoreboard;
			_scoreboard.PlayerDataById.CollectionChanged += OnCollectionChanged;
		}

		private void OnCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<NetworkConnectionToClient, DeathMatchPlayerData>> e)
		{
			if (Items.Count < _scoreboard.PlayerDataById.Count)
			{
				int difference = _scoreboard.PlayerDataById.Count - Items.Count;
				for (var i = 0; i < difference; i++)
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

			var index = 0;
			
			foreach (var (_, playerData) in _scoreboard.PlayerDataById)
			{
				Items[index].NickName.SetText(playerData.NickName);
				Items[index].KillCount.SetText(playerData.Kills.ToString());
				Items[index].DeathCount.SetText(playerData.Deaths.ToString());
				Items[index].ClassName.SetText(playerData.GameClass.ToString());
				Items[index].Avatar.texture = playerData.Avatar;
				index++;
			}
		}

		private void OnDestroy()
		{
			_scoreboard.PlayerDataById.CollectionChanged -= OnCollectionChanged;
		}
	}
}