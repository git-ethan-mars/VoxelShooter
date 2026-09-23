using R3;
using Reflex.Attributes;
using Services.ServerList;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class JoinMatchMenu : BaseMenu
	{
		private IServerListService _serverList;

		[SerializeField] private ServerListView serverListView;
		[SerializeField] private Button backButton;
		public Observable<Unit> BackButtonPressed => backButton.onClick.AsObservable();
		public Observable<Server> JoinServerButtonPressed => serverListView.JoinServerButtonPressed;

		[Inject]
		private void Construct(IServerListService serverList)
		{
			_serverList = serverList;
		}

		public override async void Show()
		{
			var servers = await _serverList.GetServersAsync(Application.exitCancellationToken);
			serverListView.Init(servers);
		}

		public override void Hide()
		{
			serverListView.Hide();
		}
	}
}