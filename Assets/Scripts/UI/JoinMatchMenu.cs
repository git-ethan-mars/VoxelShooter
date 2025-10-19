using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class JoinMatchMenu : ListView<ServerView>, IBaseMenu
	{
		private IServerListService _serverList;
		
		[SerializeField] private Button backButton;
		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		public Observable<Unit> BackButtonPressed => backButton.onClick.AsObservable();
		

		[Inject]
		private void Construct(IServerListService serverList)
		{
			_serverList = serverList;
		}

		public async void Show()
		{
			var servers = await _serverList.GetServersAsync(Application.exitCancellationToken);

			foreach (ServerInfo info in servers)
			{
				ServerView serverView = SpawnElement();
				serverView.ServerName.SetText(info.ServerTitle);
				serverView.MapName.SetText(info.MapName);
			}
			
			for (int i = servers.Count; i < Items.Count; i++)
			{
				DespawnElement(Items[i]);
			}
		}

		public void Hide()
		{
		}
	}
}