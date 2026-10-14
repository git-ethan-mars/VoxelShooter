using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class JoinMatchMenu : BaseMenu
	{
		private IServerListService _serverList;
		private CancellationTokenSource _loadCancellation;

		[SerializeField] private ServerListView serverListView;
		[SerializeField] private Button backButton;
		public Observable<Unit> BackButtonPressed => backButton.onClick.AsObservable();
		public Observable<Server> JoinServerButtonPressed => serverListView.JoinServerButtonPressed;

		[Inject]
		private void Construct(IServerListService serverList)
		{
			_serverList = serverList;
		}

		public override void Show()
		{
			CancelLoading();
			_loadCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			LoadServersAsync(_loadCancellation.Token).Forget();
		}

		public override void Hide()
		{
			CancelLoading();
			serverListView.Hide();
		}

		private async UniTaskVoid LoadServersAsync(CancellationToken cancellationToken)
		{
			List<Server> servers;
			try
			{
				servers = await _serverList.GetActiveServersAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				return;
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Failed to load the server list: {exception.Message}");
				return;
			}

			// Servers without a Steam lobby cannot be joined.
			servers.RemoveAll(server => server.SteamIDLobby == null);
			serverListView.Hide();
			serverListView.Init(servers);
		}

		private void CancelLoading()
		{
			_loadCancellation?.Cancel();
			_loadCancellation?.Dispose();
			_loadCancellation = null;
		}
	}
}
