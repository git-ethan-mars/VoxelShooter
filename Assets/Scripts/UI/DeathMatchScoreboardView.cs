using System.Linq;
using Data;
using GamePlay;
using R3;
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
			_scoreboard.Changed.Subscribe(_ => Redraw()).AddTo(this);
			Redraw();
		}

		private void Redraw()
		{
			ScoreboardEntry[] entries = _scoreboard.Entries
				.OrderByDescending(entry => entry.Kills)
				.ThenBy(entry => entry.Deaths)
				.ToArray();

			while (Items.Count < entries.Length)
			{
				SpawnElement();
			}

			while (Items.Count > entries.Length)
			{
				DespawnElement(Items[^1]);
			}

			for (int i = 0; i < entries.Length; i++)
			{
				ScoreboardEntry entry = entries[i];
				Items[i].NickName.SetText(entry.NickName);
				Items[i].KillCount.SetText(entry.Kills.ToString());
				Items[i].DeathCount.SetText(entry.Deaths.ToString());
				Items[i].ClassName.SetText(entry.GameClass.ToString());
				Items[i].Avatar.texture = _scoreboard.GetAvatar(entry.PlayerId);
			}
		}
	}
}
