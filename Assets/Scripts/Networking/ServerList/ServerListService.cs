using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Networking.ServerList
{
    public class ServerListService : IServerList
    {
        private const string ServerUrl = "http://51.250.17.30:8080/";
        private readonly string _getServersUrl = $"{ServerUrl}servers/get-servers";
        private readonly string _updateServerUrl = $"{ServerUrl}servers/update-server";
        private readonly string _deleteServerUrl = $"{ServerUrl}servers/delete-server";
        
        private static readonly HttpClient Client = new();
        private List<ServerInfo> _serversInfo;

        public ServerListService()
        {

        }
        
        public async Task GetServers()
        {
            try
            {
                await GetAsync(_getServersUrl);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error GET data: {e.Message}");
            }
        }
        
        public async Task UpdateServer(ServerInfo serverInfo)
        {
            try
            {
                await PostAsync(_updateServerUrl, serverInfo);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error POST data: {e.Message}");
            }
        }
        
        public async Task DeleteServer(ServerInfo serverInfo)
        {
            try
            {
                await PostAsync(_deleteServerUrl, serverInfo);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error POST data: {e.Message}");
            }
        }

        private async Task GetAsync(string url)
        {
            var response = await Client.GetAsync(url);
            _serversInfo = DeserializeServersData(await response.Content.ReadAsStringAsync());
        }

        private async Task PostAsync(string url, ServerInfo serverInfo)
        {
            var content = new StringContent(JsonUtility.ToJson(serverInfo), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            _serversInfo = DeserializeServersData(await response.Content.ReadAsStringAsync());
        }

        private List<ServerInfo> DeserializeServersData(string data)
        {
            var serversInfo = JsonConvert.DeserializeObject<List<string>>(data);
            return serversInfo.Select(JsonConvert.DeserializeObject<ServerInfo>).ToList();
        }

        public List<ServerInfo> GetServersInfo()
        {
            return _serversInfo;
        }
    }
}