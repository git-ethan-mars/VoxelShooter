using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
namespace Services
{
	public class ServerListService : IServerListService
	{
		private const string HostName = "http://127.0.0.1:8000";
		private const string ServersEndpoint = "servers";

		private Uri BaseUri => new Uri(HostName);

		public async UniTask<List<ServerInfo>> GetServersAsync(CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, ServersEndpoint);
			using UnityWebRequest request = await UnityWebRequest.Get(path).SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

			if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				throw new InvalidOperationException("Unable to connect to server");
			}

			var servers = JsonConvert.DeserializeObject<JArray>(request.downloadHandler.text)
				.Select(x => x.ToObject<ServerInfo>())
				.ToList();
			return servers;
		}

		public async UniTask CreateServerAsync(ServerInfo server, CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, ServersEndpoint);
			string json = JsonConvert.SerializeObject(server);

			UnityWebRequest request = null;

			try
			{
				request = await UnityWebRequest.Post(path, json, "application/json").SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
			}
			catch (UnityWebRequestException)
			{
				Debug.LogWarning("Can't connect to server list");
			}
			finally
			{
				request?.Dispose();
			}
		}

		public async UniTask RemoveServerAsync(ServerInfo server, CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, $"{ServersEndpoint}/{server.OwnerId}");
			UnityWebRequest request = null;

			try
			{
				request = await UnityWebRequest.Delete(path).SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
			}
			catch (UnityWebRequestException)
			{
				Debug.LogWarning("Can't connect to server list");
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				request?.Dispose();
			}
		}
	}
}