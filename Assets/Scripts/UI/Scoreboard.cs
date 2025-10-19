using System.Collections.Generic;
using Data;
using UnityEngine;
namespace UI
{
	public class ScoreboardView : ListView<ScoreboardRecord>
	{
		[field:SerializeField] public CanvasGroup CanvasGroup { get; private set; }

		public void UpdateScoreboard(List<PlayerData> scoreboardData)
		{
			if (Items.Count < scoreboardData.Count)
			{
				int difference = scoreboardData.Count - Items.Count;
				for (var i = 0; i < difference; i++)
				{
					SpawnElement();
				}
			}
			else
			{
				for (int i = scoreboardData.Count; i < Items.Count; i++)
				{
					DespawnElement(Items[i]);
				}
			}

			for (var i = 0; i < scoreboardData.Count; i++)
			{
				Items[i].NickName.SetText(scoreboardData[i].NickName);
				Items[i].KillCount.SetText(scoreboardData[i].Kills.ToString());
				Items[i].DeathCount.SetText(scoreboardData[i].Deaths.ToString());
				Items[i].ClassName.SetText(scoreboardData[i].GameClass.ToString());
				Items[i].Avatar.texture = scoreboardData[i].Avatar;
			}
		}
	}
}