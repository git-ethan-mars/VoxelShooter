using System;
using System.Text;
using AOT;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Networking
{
	public class SteamLobby : MonoBehaviour
	{
		private const string HostAddressKey = "HostAddress";
		protected Callback<GameLobbyJoinRequested_t> JoinRequested;
		private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

		private void Awake()
		{
			JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);

			_steamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
		}

		private void Update()
		{
			SteamAPI.RunCallbacks();
		}

		private void OnDestroy()
		{
			SteamAPI.Shutdown();
		}

		public async UniTask<CSteamID> CreateLobbyAsync(int maxPlayers)
		{
			var tcs = new UniTaskCompletionSource<CSteamID>();
			using var lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(callback =>
			{
				if (callback.m_eResult != EResult.k_EResultOK)
				{
					tcs.TrySetException(new InvalidOperationException(callback.m_eResult.ToString()));
				}
				else
				{
					SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey,
						SteamUser.GetSteamID().ToString());
					tcs.TrySetResult(new CSteamID(callback.m_ulSteamIDLobby));
				}
			});

			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxPlayers);
			return await tcs.Task;
		}

		public async UniTask<string> JoinLobbyAsync(CSteamID steamLobbyId)
		{
			var tsc = new UniTaskCompletionSource<string>();

			using var lobbyEnteredCallback = Callback<LobbyEnter_t>.Create(_ =>
			{
				string networkAddress = SteamMatchmaking.GetLobbyData(steamLobbyId, HostAddressKey);
				tsc.TrySetResult(networkAddress);
			});

			SteamMatchmaking.JoinLobby(steamLobbyId);
			return await tsc.Task;
		}

		public void LeaveLobby(CSteamID steamLobbyId)
		{
			SteamMatchmaking.LeaveLobby(steamLobbyId);
		}

		[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}

		private void OnJoinRequest(GameLobbyJoinRequested_t callback)
		{
			Debug.Log("FRIENDLIST");
		}
	}
}
