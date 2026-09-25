using Cysharp.Threading.Tasks;
using Networking;
using Services;
using Steamworks;
using UnityEngine;

namespace Infrastructure.States
{
	public class JoinSteamLobbyState : IPayloadedState<Server>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly VSNetworkManager _networkManager;

		public JoinSteamLobbyState(GameStateMachine gameStateMachine, VSNetworkManager networkManager)
		{
			_gameStateMachine = gameStateMachine;
			_networkManager = networkManager;
		}

		public async UniTask EnterAsync(Server server)
		{
			SteamLobby steamLobby = _networkManager.GetComponent<SteamLobby>();
			string networkAddress = await steamLobby.JoinLobbyAsync(new CSteamID(server.SteamIDLobby));

			if (string.IsNullOrEmpty(networkAddress))
			{
				Debug.Log("Wrong network address");
				await _gameStateMachine.EnterAsync<GameMenuState>();
				return;
			}

			_networkManager.networkAddress = networkAddress;
			await _gameStateMachine.EnterAsync<InitializeClientState>();
		}

		public void Exit()
		{
		}
	}
}
