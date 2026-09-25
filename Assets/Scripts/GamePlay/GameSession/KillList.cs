using Data;
using Mirror;
using R3;

namespace GamePlay
{
	public class KillList
	{
		public readonly SyncList<KillData> Kills = new SyncList<KillData>();

		private readonly Subject<KillData> _killAdded = new Subject<KillData>();

		public Observable<KillData> KillAdded => _killAdded;

		public void AddKill(KillData kill)
		{
			if (kill is null)
			{
				throw new System.ArgumentNullException(nameof(kill));
			}

			Kills.Add(kill);
			_killAdded.OnNext(kill);
		}
	}
}
