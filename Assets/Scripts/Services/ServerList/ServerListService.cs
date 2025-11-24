using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
namespace Services.ServerList
{
	public class ServerListService : IServerListService
	{
		private const string HostName = "http://192.168.0.26:8000";
		private const string ServersEndpoint = "servers";

		private string _token;

		private Uri BaseUri => new Uri(HostName);

		public async UniTask<List<Server>> GetServersAsync(CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, ServersEndpoint);
			using UnityWebRequest request = await UnityWebRequest.Get(path).SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

			if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				throw new InvalidOperationException("Unable to connect to server");
			}

			var servers = JsonConvert.DeserializeObject<JArray>(request.downloadHandler.text)
				.Select(x => x.ToObject<Server>())
				.ToList();
			return servers;
		}

		public async UniTask<Server> CreateNewServerAsync(ulong steamIDLobby, int availableSlots, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, string>()
			{
				["steam_id_lobby"] = steamIDLobby.ToString(),
				["available_slots"] = availableSlots.ToString(),
			};
			string relativeUrl = AddQueryParameters($"{ServersEndpoint}/new", parameters);
			
			var path = new Uri(BaseUri, relativeUrl);

			UnityWebRequest request = await UnityWebRequest.Get(path).SendWebRequest()
				.ToUniTask(cancellationToken: cancellationToken);

			if (request.result != UnityWebRequest.Result.Success)
			{
				throw new InvalidOperationException(request.error);
			}

			var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
			_token = result["token"].ToString();
			var server = JsonConvert.DeserializeObject<Server>(result["server"].ToString());
			return server;
		}

		public async UniTask RemoveServerAsync(long serverID, CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, $"{ServersEndpoint}/{serverID}");

			using UnityWebRequest request = UnityWebRequest.Delete(path);
			request.SetRequestHeader("Authorization", $"Bearer {_token}");
			await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();

			if (request.result != UnityWebRequest.Result.Success)
			{
				throw new InvalidOperationException(request.error);
			}
		}

		public async UniTask UpdateServerAsync(Server server, CancellationToken cancellationToken)
		{
			var path = new Uri(BaseUri, $"{ServersEndpoint}/{server.ServerID}");

			string json = JsonConvert.SerializeObject(server);

			UnityWebRequest request = UnityWebRequest.Put(path, json);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", $"Bearer {_token}");
			await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();
		}
		
		private string AddQueryParameters(string baseUri, Dictionary<string, string> parameters)
		{
			var uriBuilder = new StringBuilder(baseUri);
			bool first = true;
			foreach (var param in parameters)
			{
				// Use Uri.EscapeDataString to properly URL-encode the keys and values
				string key = UnityWebRequest.EscapeURL(param.Key);
				string value = UnityWebRequest.EscapeURL(param.Value);
            
				uriBuilder.Append(first ? '?' : '&');
				uriBuilder.AppendFormat("{0}={1}", key, value);
				first = false;
			}
			return uriBuilder.ToString();
		}
	}
}