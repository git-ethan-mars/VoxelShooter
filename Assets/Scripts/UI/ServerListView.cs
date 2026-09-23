using System;
using System.Collections.Generic;
using R3;
using Services;

namespace UI
{
	public class ServerListView : ListView<ServerView>
	{
		private readonly Subject<Server> _joinServerButtonPressed = new Subject<Server>();
		private readonly List<IDisposable> _disposables = new List<IDisposable>();

		public Observable<Server> JoinServerButtonPressed => _joinServerButtonPressed;

		private void OnDestroy()
		{
			for (int i = 0; i < _disposables.Count; i++)
			{
				_disposables[i].Dispose();
			}

			_disposables.Clear();
		}

		public void Init(List<Server> servers)
		{
			for (int i = 0; i < servers.Count; i++)
			{
				Server server = servers[i];
				ServerView serverView = SpawnElement();
				serverView.ServerName.SetText(server.ServerTitle);
				serverView.MapName.SetText(server.MapName);
				_disposables.Add(serverView.JoinButtonPressed.Subscribe(_ => OnServerJoinButtonPressed(server)));
			}
		}

		public void Hide()
		{
			Clear();

			for (int i = 0; i < _disposables.Count; i++)
			{
				_disposables[i].Dispose();
			}

			_disposables.Clear();
		}

		private void OnServerJoinButtonPressed(Server server)
		{
			_joinServerButtonPressed.OnNext(server);
		}
	}
}
