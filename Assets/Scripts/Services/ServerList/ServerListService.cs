using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Services
{
	public class ServerListService : IServerListService
	{
		private const string HostName = "http://localhost:8000";
		private const string ServersEndpoint = "servers";
		private const string ActiveServersEndpoint = "servers/active";

		private readonly Uri _baseUri = new Uri(HostName);

		// Bearer token of the server created by this client, required for update and removal.
		private string _token;

		public async UniTask<List<Server>> GetActiveServersAsync(CancellationToken cancellationToken)
		{
			string response = await SendAsync(UnityWebRequest.kHttpVerbGET, ActiveServersEndpoint, null, false, cancellationToken);
			return JsonConvert.DeserializeObject<List<Server>>(response);
		}

		public async UniTask<Server> CreateServerAsync(ServerCreateRequest serverCreate, CancellationToken cancellationToken)
		{
			string response = await SendAsync(UnityWebRequest.kHttpVerbPOST, ServersEndpoint, serverCreate, false, cancellationToken);
			var serverCreated = JsonConvert.DeserializeObject<ServerCreatedResponse>(response);
			_token = serverCreated.Token;
			return serverCreated.Server;
		}

		public async UniTask<Server> UpdateServerAsync(long serverID, ServerUpdateRequest serverUpdate, CancellationToken cancellationToken)
		{
			string response = await SendAsync("PATCH", $"{ServersEndpoint}/{serverID}", serverUpdate, true, cancellationToken);
			return JsonConvert.DeserializeObject<Server>(response);
		}

		public async UniTask RemoveServerAsync(long serverID, CancellationToken cancellationToken)
		{
			await SendAsync(UnityWebRequest.kHttpVerbDELETE, $"{ServersEndpoint}/{serverID}", null, true, cancellationToken);
		}

		private async UniTask<string> SendAsync(string method, string relativeUrl, object body, bool authorize,
			CancellationToken cancellationToken)
		{
			using var request = new UnityWebRequest(new Uri(_baseUri, relativeUrl), method);
			request.downloadHandler = new DownloadHandlerBuffer();

			if (body != null)
			{
				request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body)));
				request.SetRequestHeader("Content-Type", "application/json");
			}

			if (authorize)
			{
				request.SetRequestHeader("Authorization", $"Bearer {_token}");
			}

			// Throws UnityWebRequestException on connection errors and non-2xx responses.
			await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
			return request.downloadHandler.text;
		}

		private record ServerCreatedResponse
		{
			[JsonProperty("server")] public Server Server { get; private set; }

			[JsonProperty("token")] public string Token { get; private set; }
		}
	}
}
