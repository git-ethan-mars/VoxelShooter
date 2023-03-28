using Mirror;
using UnityEngine;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Core
{
    public class CustomNetworkManager : NetworkManager
    {
        public Transform spawnPoint;
        public static Map Map { get; private set; }
        public static byte[] CompressedMap { get; private set; }

        public override void OnStartHost()
        {
            base.OnStartHost();
            Map = MapReader.ReadFromFile("AncientEgypt.vxl");
            CompressedMap = MapCompressor.Compress(Map);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }
}